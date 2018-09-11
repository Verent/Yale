/*
 * ProductionPatternAlternative.cs
 *
 * This program is free software: you can redistribute it and/or
 * modify it under the terms of the BSD license.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * LICENSE.txt file for more details.
 *
 * Copyright (c) 2003-2015 Per Cederberg. All rights reserved.
 */

using System;
using System.Collections;
using System.Text;

namespace PerCederberg.Grammatica.Runtime {

    /**
     * A production pattern alternative. This class represents a list of
     * production pattern elements. In order to provide productions that
     * cannot be represented with the element occurance counters, multiple
     * alternatives must be created and added to the same production
     * pattern. A production pattern alternative is always contained
     * within a production pattern.
     *
     * @author   Per Cederberg
     * @version  1.5
     */
    public class ProductionPatternAlternative {

        /**
         * The production pattern.
         */
        private ProductionPattern pattern;

        /**
         * The element list.
         */
        private ArrayList elements = new ArrayList();

        /**
         * The look-ahead set associated with this alternative.
         */
        private LookAheadSet lookAhead = null;

        /**
         * Creates a new production pattern alternative.
         */
        public ProductionPatternAlternative() {
        }

        /**
         * The production pattern property (read-only). This property
         * contains the pattern having this alternative.
         *
         * @since 1.5
         */
        public ProductionPattern Pattern {
            get {
                return pattern;
            }
        }

        /**
         * Returns the production pattern containing this alternative.
         *
         * @return the production pattern for this alternative
         *
         * @see #Pattern
         *
         * @deprecated Use the Pattern property instead.
         */
        public ProductionPattern GetPattern() {
            return Pattern;
        }

        /**
         * The look-ahead set property. This property contains the
         * look-ahead set associated with this alternative.
         */
        internal LookAheadSet LookAhead {
            get {
                return lookAhead;
            }
            set {
                lookAhead = value;
            }
        }

        /**
         * The production pattern element count property (read-only).
         *
         * @since 1.5
         */
        public int Count {
            get {
                return elements.Count;
            }
        }

        /**
         * Returns the number of elements in this alternative.
         *
         * @return the number of elements in this alternative
         *
         * @see #Count
         *
         * @deprecated Use the Count property instead.
         */
        public int GetElementCount() {
            return Count;
        }

        /**
         * The production pattern element index (read-only).
         *
         * @param index          the element index, 0 <= pos < Count
         *
         * @return the element found
         *
         * @since 1.5
         */
        public ProductionPatternElement this[int index] {
            get {
                return (ProductionPatternElement) elements[index];
            }
        }

        /**
         * Returns an element in this alternative.
         *
         * @param pos            the element position, 0 <= pos < count
         *
         * @return the element found
         *
         * @deprecated Use the class indexer instead.
         */
        public ProductionPatternElement GetElement(int pos) {
            return this[pos];
        }

        /**
         * Checks if this alternative is recursive on the left-hand
         * side. This method checks all the possible left side
         * elements and returns true if the pattern itself is among
         * them.
         *
         * @return true if the alternative is left side recursive, or
         *         false otherwise
         */
        public bool IsLeftRecursive() {
            ProductionPatternElement  elem;

            for (int i = 0; i < elements.Count; i++) {
                elem = (ProductionPatternElement) elements[i];
                if (elem.Id == pattern.Id) {
                    return true;
                } else if (elem.MinCount > 0) {
                    break;
                }
            }
            return false;
        }

        /**
         * Checks if this alternative is recursive on the right-hand side.
         * This method checks all the possible right side elements and
         * returns true if the pattern itself is among them.
         *
         * @return true if the alternative is right side recursive, or
         *         false otherwise
         */
        public bool IsRightRecursive() {
            ProductionPatternElement  elem;

            for (int i = elements.Count - 1; i >= 0; i--) {
                elem = (ProductionPatternElement) elements[i];
                if (elem.Id == pattern.Id) {
                    return true;
                } else if (elem.MinCount > 0) {
                    break;
                }
            }
            return false;
        }

        /**
         * Checks if this alternative would match an empty stream of
         * tokens. This check is equivalent of getMinElementCount()
         * returning zero (0).
         *
         * @return true if the rule can match an empty token stream, or
         *         false otherwise
         */
        public bool IsMatchingEmpty() {
            return GetMinElementCount() == 0;
        }

        /**
         * Changes the production pattern containing this alternative.
         * This method should only be called by the production pattern
         * class.
         *
         * @param pattern        the new production pattern
         */
        internal void SetPattern(ProductionPattern pattern) {
            this.pattern = pattern;
        }

        /**
         * Returns the minimum number of elements needed to satisfy
         * this alternative. The value returned is the sum of all the
         * elements minimum count.
         *
         * @return the minimum number of elements
         */
        public int GetMinElementCount() {
            ProductionPatternElement  elem;
            int                       min = 0;

            for (int i = 0; i < elements.Count; i++) {
                elem = (ProductionPatternElement) elements[i];
                min += elem.MinCount;
            }
            return min;
        }

        /**
         * Returns the maximum number of elements needed to satisfy
         * this alternative. The value returned is the sum of all the
         * elements maximum count.
         *
         * @return the maximum number of elements
         */
        public int GetMaxElementCount() {
            ProductionPatternElement  elem;
            int                       max = 0;

            for (int i = 0; i < elements.Count; i++) {
                elem = (ProductionPatternElement) elements[i];
                if (elem.MaxCount >= Int32.MaxValue) {
                    return Int32.MaxValue;
                } else {
                    max += elem.MaxCount;
                }
            }
            return max;
        }

        /**
         * Adds a token to this alternative. The token is appended to
         * the end of the element list. The multiplicity values
         * specified define if the token is optional or required, and
         * if it can be repeated.
         *
         * @param id             the token (pattern) id
         * @param min            the minimum number of occurancies
         * @param max            the maximum number of occurancies, or
         *                       -1 for infinite
         */
        public void AddToken(int id, int min, int max) {
            AddElement(new ProductionPatternElement(true, id, min, max));
        }

        /**
         * Adds a production to this alternative. The production is
         * appended to the end of the element list. The multiplicity
         * values specified define if the production is optional or
         * required, and if it can be repeated.
         *
         * @param id             the production (pattern) id
         * @param min            the minimum number of occurancies
         * @param max            the maximum number of occurancies, or
         *                       -1 for infinite
         */
        public void AddProduction(int id, int min, int max) {
            AddElement(new ProductionPatternElement(false, id, min, max));
        }

        /**
         * Adds a production pattern element to this alternative. The
         * element is appended to the end of the element list.
         *
         * @param elem           the production pattern element
         */
        public void AddElement(ProductionPatternElement elem) {
            elements.Add(elem);
        }

        /**
         * Adds a production pattern element to this alternative. The
         * multiplicity values in the element will be overridden with
         * the specified values. The element is appended to the end of
         * the element list.
         *
         * @param elem           the production pattern element
         * @param min            the minimum number of occurancies
         * @param max            the maximum number of occurancies, or
         *                       -1 for infinite
         */
        public void AddElement(ProductionPatternElement elem,
                               int min,
                               int max) {

            if (elem.IsToken()) {
                AddToken(elem.Id, min, max);
            } else {
                AddProduction(elem.Id, min, max);
            }
        }

        /**
         * Checks if this object is equal to another. This method only
         * returns true for another production pattern alternative
         * with identical elements in the same order.
         *
         * @param obj            the object to compare with
         *
         * @return true if the object is identical to this one, or
         *         false otherwise
         */
        public override bool Equals(object obj) {
            if (obj is ProductionPatternAlternative) {
                return Equals((ProductionPatternAlternative) obj);
            } else {
                return false;
            }
        }

        /**
         * Checks if this alternative is equal to another. This method
         * returns true if the other production pattern alternative
         * has identical elements in the same order.
         *
         * @param alt            the alternative to compare with
         *
         * @return true if the object is identical to this one, or
         *         false otherwise
         */
        public bool Equals(ProductionPatternAlternative alt) {
            if (elements.Count != alt.elements.Count) {
                return false;
            }
            for (int i = 0; i < elements.Count; i++) {
                if (!elements[i].Equals(alt.elements[i])) {
                    return false;
                }
            }
            return true;
        }

        /**
         * Returns a hash code for this object.
         *
         * @return a hash code for this object
         */
        public override int GetHashCode() {
            return elements.Count.GetHashCode();
        }

        /**
         * Returns a string representation of this object.
         *
         * @return a token string representation
         */
        public override string ToString() {
            StringBuilder  buffer = new StringBuilder();

            for (int i = 0; i < elements.Count; i++) {
                if (i > 0) {
                    buffer.Append(" ");
                }
                buffer.Append(elements[i]);
            }
            return buffer.ToString();
        }
    }
}
