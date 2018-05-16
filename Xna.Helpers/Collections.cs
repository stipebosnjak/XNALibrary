#region

using System;
using System.Collections.Generic;

#endregion

namespace Xna.Helpers
{

    #region Enums

    #endregion
    /// <summary>
    /// Class that contains altered collections 
    /// </summary>
    public static class Collections
    {
        #region Classes

        #region DictionaryV2
        /// <summary>
        /// Generic Dicitonary 
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="O"></typeparam>
        public class DictionaryV2<K, O>
        {
            #region Fields

            private static Random _random;
            private Dictionary<K, O> _dictionary;

            #endregion

            #region Properties
            /// <summary>
            /// Returns dictionary count
            /// </summary>
            public int Count
            {
                get { return _dictionary.Count; }
            }
            /// <summary>
            /// Dictionary
            /// </summary>
            public Dictionary<K, O> Dictionary
            {
                get { return _dictionary; }
                set { _dictionary = value; }
            }

            #endregion

            #region Constructors

            static DictionaryV2()
            {
                _random = new Random();
            }
            /// <summary>
            /// Creates a new dictionary
            /// </summary>
            public DictionaryV2()
            {
                _dictionary = new Dictionary<K, O>();
            }

            #endregion

            #region Methods
            /// <summary>
            /// Gets random object
            /// </summary>
            /// <returns></returns>
            public O GetRandomObject()
            {
                var keyList = new List<K>(_dictionary.Keys);
                int randomPointOfInt = Generators.RandomNumber(0, keyList.Count - 1);
                K key = keyList[randomPointOfInt];
                return _dictionary[key];
            }
            /// <summary>
            /// Adds new object to dictionary
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public void Add(K key,O value)
            {
                _dictionary.Add(key,value);
            }

            /// <summary>
            /// Removes object to dictionary
            /// </summary>
            /// <param name="key"></param>
            public void Remove(K key)
            {
                _dictionary.Remove(key);
            }
            #endregion
        }

        #endregion

        #endregion

        #region v

        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Constructors

        #endregion

        #region Methods

        #endregion

        #endregion
    }
}