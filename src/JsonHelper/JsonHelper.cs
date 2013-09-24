//-----------------------------------------------------------------------
// <copyright file="JsonHelper.cs" company="Etersoul">
//    Copyright (c) 2013, Etersoul.
//
//    Licensed under the MIT License (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.opensource.org/licenses/mit-license.php
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>
// <author>William (etersoul)</author>
// <website>https://github.com/Etersoul/sharp-json-helper</website>
//-----------------------------------------------------------------------
namespace JsonHelper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// JSON helper.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// The current object in the iteration.
        /// </summary>
        private static object current = null;

        /// <summary>
        /// Decodes the JSON string to the selected type.
        /// </summary>
        /// <returns>The JSON object in form of T.</returns>
        /// <param name="json">The JSON string.</param>
        /// <typeparam name="T">The data type you want the function to return as.</typeparam>
        public static T DecodeJson<T>(string json)
        {
            Stack<char> stack = new Stack<char>();
            bool literal = false;
            bool dec = false;
            bool key = false;
            bool lastIsObject = false;
            string keyString = string.Empty;
            string valString = string.Empty;
            StringBuilder builder = new StringBuilder();
            Stack<object> globalObjectStack = new Stack<object>();
            Hashtable currentObject = null;
            ArrayList currentArray = null;

            object root = null;
            current = null;

            for (int i = 0; i < json.Length; i++)
            {
                if (!literal)
                {
                    switch (json[i])
                    {
                        case '{':
                            stack.Push(json[i]);
                            if (root != null)
                            {
                                globalObjectStack.Push(current);

                                currentObject = new Hashtable();
                                InsertToCurrentObject(currentObject, keyString);
                            }
                            else
                            {
                                root = new Hashtable();
                                currentObject = (Hashtable)root;
                            }

                            key = true;
                            keyString = string.Empty;

                            current = currentObject;
                            break;

                        case '[':
                            stack.Push(json[i]);
                            if (root != null)
                            {
                                globalObjectStack.Push(current);

                                currentArray = new ArrayList();
                                InsertToCurrentObject(currentArray, keyString);
                            }
                            else
                            {
                                root = new ArrayList();
                                currentArray = (ArrayList)root;
                            }

                            key = false;
                            keyString = string.Empty;

                            current = currentArray;
                            break;

                        case '"':
                            literal = true;

                            builder = new StringBuilder();
                            break;

                        case ':':
                            if (key)
                            {
                                key = false;
                            }

                            builder = new StringBuilder();

                            break;

                        case ',':
                            char last = stack.Peek();
                            if (!lastIsObject)
                            {
                                if (last == '{')
                                {
                                    if (!dec)
                                    {
                                        InsertToCurrentObject(valString, keyString);
                                    }
                                    else
                                    {
                                        InsertToCurrentObject(decimal.Parse(builder.ToString()), keyString);
                                    }
                                }
                                else if (last == '[')
                                {
                                    if (!dec)
                                    {
                                        InsertToCurrentObject(valString);
                                    }
                                    else
                                    {
                                        InsertToCurrentObject(decimal.Parse(builder.ToString()));
                                    }
                                }
                            }

                            if (last == '{')
                            {
                                key = true;
                            }

                            dec = false;
                            lastIsObject = false;

                            break;

                        case '}':
                            if (!lastIsObject)
                            {
                                if (!dec)
                                {
                                    InsertToCurrentObject(valString, keyString);
                                }
                                else
                                {
                                    InsertToCurrentObject(decimal.Parse(builder.ToString()), keyString);
                                }
                            }

                            char popCurly = stack.Pop();
                            if (popCurly != '{')
                            {
                                throw new Exception("Unexpected closing curly bracket.");
                            }

                            key = false;
                            dec = false;
                            keyString = string.Empty;
                            valString = string.Empty;

                            lastIsObject = true;

                            if (!current.Equals(root))
                            {
                                current = globalObjectStack.Pop();
                            }

                            break;

                        case ']':
                            if (!lastIsObject)
                            {
                                if (!dec)
                                {
                                    InsertToCurrentObject(valString);
                                }
                                else
                                {
                                    InsertToCurrentObject(decimal.Parse(builder.ToString()));
                                }
                            }

                            char popSquare = stack.Pop();
                            if (popSquare != '[')
                            {
                                throw new Exception("Unexpected closing square bracket.");
                            }

                            dec = false;
                            valString = string.Empty;

                            lastIsObject = true;

                            if (!current.Equals(root))
                            {
                                current = globalObjectStack.Pop();
                            }

                            break;

                        default:
                            if (Array.IndexOf(new char[11] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' }, json[i]) != -1)
                            {
                                if (!dec)
                                {
                                    builder = new StringBuilder();
                                }

                                dec = true;
                                builder.Append(json[i]);
                            }

                            break;
                    }
                }
                else
                {
                    if (json[i] == '"')
                    {
                        if (key)
                        {
                            keyString = builder.ToString();
                            builder = new StringBuilder();
                        }
                        else
                        {
                            valString = builder.ToString();
                            builder = new StringBuilder();
                        }

                        literal = false;
                    }
                    else
                    {
                        if (json[i] == '\\' && json[i + 1] == '"')
                        {
                            i++;
                        }

                        builder.Append(json[i]);
                    }
                }
            }

            if (stack.Count != 0)
            {
                throw new Exception("Expecting " + stack.Pop());
            }

            return (T)root;
        }

        /// <summary>
        /// Inserts to current object.
        /// </summary>
        /// <param name="anything">Anything that you want to insert (int, string, Hashtable, ArrayList).</param>
        private static void InsertToCurrentObject(object anything)
        {
            InsertToCurrentObject(anything, string.Empty);
        }

        /// <summary>
        /// Inserts to current object, with key (useful for Hashtable).
        /// </summary>
        /// <param name="anything">Anything that you want to insert (int, string, Hashtable, Arraylist).</param>
        /// <param name="key">The key for the Hashtable data type.</param>
        private static void InsertToCurrentObject(object anything, string key)
        {
            if (current is Hashtable && key != string.Empty)
            {
                ((Hashtable)current).Add(key, anything);
            }
            else if (current is ArrayList)
            {
                ((ArrayList)current).Add(anything);
            }
        }
    }
}
