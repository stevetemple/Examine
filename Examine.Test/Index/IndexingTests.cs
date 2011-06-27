﻿using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;

using Lucene.Net.Analysis.Standard;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Examine.Test.Index
{
    [TestClass]
    public class IndexingTests
    {
        private DirectoryInfo _currentFolder;

        [TestMethod]
        public void Indexing_Item_Indexed()
        {
            //arrange

            var indexer = GetIndexer(new[] {new IndexFieldDefinition {Name = "Field1"}});

            //act

            indexer.ReIndexNode(new IndexItem
                {
                    Fields = new Dictionary<string, string> {{"Field1", "hello world"}},
                    Id = "test1",
                    ItemType = "test"
                }, "TestCategory");

            //assert

            var searcher = GetSearcher(_currentFolder);
            var results = searcher.Search(searcher.CreateSearchCriteria().Field("Field1", "hello world").Compile());

            Assert.AreEqual(1, results.Count());

        }

        [TestMethod]
        public void Indexing_Special_Fields_Indexed()
        {
            //arrange

            var indexer = GetIndexer(new[] { new IndexFieldDefinition { Name = "Field1" } });

            //act

            indexer.ReIndexNode(new IndexItem
            {
                Fields = new Dictionary<string, string> { { "Field1", "hello world" } },
                Id = "test1",
                ItemType = "test"
            }, "TestCategory");

            //assert

            var searcher = GetSearcher(_currentFolder);
            var results = searcher.Search(searcher.CreateSearchCriteria().Field("Field1", "hello world").Compile());

            Assert.AreEqual(3, results.First().Fields.Count());
            Assert.AreEqual("test1", results.First().Fields[LuceneIndexer.IndexNodeIdFieldName]);
            Assert.AreEqual("testcategory", results.First().Fields[LuceneIndexer.IndexCategoryFieldName]);
        }

        private ISearcher GetSearcher(DirectoryInfo folder)
        {
            var searcher = new LuceneSearcher(folder, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
            return searcher;
        }

        private IIndexer GetIndexer(IEnumerable<IndexFieldDefinition> fields)
        {
            _currentFolder = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString()));
            _currentFolder.Create();
            var indexer = new LuceneIndexer(
                new IndexCriteria(fields, null, null),
                _currentFolder,
                new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29), SynchronizationType.SingleThreaded);
            return indexer;
        }
    }
}