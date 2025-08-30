using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Application.Tests.TestUtils
{
    /// <summary>
    /// Utilidades para hacer assertions más expresivas y reutilizables
    /// </summary>
    public static class AssertionUtils
    {
        /// <summary>
        /// Verifica que una PaginationResponse tenga los valores esperados
        /// </summary>
        public static void AssertPaginationResponse<T>(
            PaginationResponse<T> actual,
            int expectedCount,
            int expectedTotalPages,
            int expectedTotalRows)
        {
            Assert.NotNull(actual);
            Assert.NotNull(actual.Data);
            Assert.Equal(expectedCount, actual.Data.Count);
            Assert.Equal(expectedTotalPages, actual.TotalPages);
            Assert.Equal(expectedTotalRows, actual.TotalRows);
        }

        /// <summary>
        /// Verifica que una lista contenga elementos con los IDs esperados
        /// </summary>
        public static void AssertContainsIds<T>(
            IEnumerable<T> collection,
            Func<T, int> idSelector,
            params int[] expectedIds)
        {
            var actualIds = collection.Select(idSelector).ToList();
            
            foreach (var expectedId in expectedIds)
            {
                Assert.Contains(expectedId, actualIds);
            }
        }

        /// <summary>
        /// Verifica que una cadena contenga caracteres especiales específicos
        /// </summary>
        public static void AssertContainsSpecialCharacters(string text, params string[] specialChars)
        {
            Assert.NotNull(text);
            
            foreach (var specialChar in specialChars)
            {
                Assert.Contains(specialChar, text);
            }
        }

        /// <summary>
        /// Verifica que dos fechas estén dentro de un rango de tolerancia
        /// </summary>
        public static void AssertDateTimeWithinTolerance(
            DateTime expected,
            DateTime actual,
            TimeSpan tolerance)
        {
            var difference = Math.Abs((expected - actual).TotalMilliseconds);
            var toleranceMs = tolerance.TotalMilliseconds;
            
            Assert.True(difference <= toleranceMs,
                $"Expected date {expected:O} but got {actual:O}. " +
                $"Difference: {difference}ms, Tolerance: {toleranceMs}ms");
        }

        /// <summary>
        /// Verifica que una colección esté ordenada según un selector
        /// </summary>
        public static void AssertIsOrdered<T, TKey>(
            IEnumerable<T> collection,
            Func<T, TKey> keySelector,
            bool ascending = true) where TKey : IComparable<TKey>
        {
            var list = collection.ToList();
            
            if (list.Count <= 1) return;

            for (int i = 0; i < list.Count - 1; i++)
            {
                var current = keySelector(list[i]);
                var next = keySelector(list[i + 1]);
                
                int comparison = current.CompareTo(next);
                
                if (ascending)
                {
                    Assert.True(comparison <= 0, 
                        $"Collection is not ordered ascending. Item at index {i} ({current}) > item at index {i + 1} ({next})");
                }
                else
                {
                    Assert.True(comparison >= 0, 
                        $"Collection is not ordered descending. Item at index {i} ({current}) < item at index {i + 1} ({next})");
                }
            }
        }

        /// <summary>
        /// Verifica que todos los elementos de una colección cumplan una condición
        /// </summary>
        public static void AssertAll<T>(
            IEnumerable<T> collection,
            Func<T, bool> predicate,
            string failureMessage = "Not all items satisfy the condition")
        {
            var items = collection.ToList();
            var failedItems = items.Where(x => !predicate(x)).ToList();
            
            Assert.True(!failedItems.Any(),
                $"{failureMessage}. Failed items count: {failedItems.Count}");
        }

        /// <summary>
        /// Verifica el mapeo de propiedades entre objetos del dominio y DTOs
        /// </summary>
        public static void AssertMapping<TSource, TDto>(
            TSource source,
            TDto dto,
            params (Func<TSource, object> sourceSelector, Func<TDto, object> dtoSelector, string propertyName)[] mappings)
        {
            Assert.NotNull(source);
            Assert.NotNull(dto);

            foreach (var (sourceSelector, dtoSelector, propertyName) in mappings)
            {
                var sourceValue = sourceSelector(source);
                var dtoValue = dtoSelector(dto);
                
                Assert.Equal(sourceValue, dtoValue);
            }
        }
    }

    /// <summary>
    /// Utilidades específicas para paginación
    /// </summary>
    public static class PaginationTestUtils
    {
        public static PaginationParams CreateParams(int pageNumber = 1, int pageSize = 10) =>
            new() { PageNumber = pageNumber, PageSize = pageSize };

        public static PaginationResponse<T> CreateResponse<T>(
            IList<T> data,
            int totalPages,
            int totalRows) =>
            new()
            {
                Data = data.ToList(),
                TotalPages = totalPages,
                TotalRows = totalRows
            };

        public static PaginationResponse<T> CreateEmptyResponse<T>() =>
            new()
            {
                Data = new List<T>(),
                TotalPages = 0,
                TotalRows = 0
            };
    }
}