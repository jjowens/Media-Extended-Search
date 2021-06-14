using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Umbraco.Web.WebApi;
using Umbraco.Web.Editors;
using Umbraco.Core.Persistence;
using Umbraco.Web.Mvc;
using My.MediaExtendedSearch.Models;
using Umbraco.Web;
using Examine;
using Examine.SearchCriteria;
using System.Web.Http;

namespace My.MediaExtendedSearch
{
    [PluginController("MediaExtendedSearchService")]
    public class MediaSearchController : UmbracoAuthorizedApiController
    {
        
        [AcceptVerbs("POST")]
        [HttpPost]
        public MediaDetailsDTO SearchExamine(SearchParameters search)
        {
            // CHECK IF EXAMINE SEARCHER EXISTS
            if (string.IsNullOrEmpty(search.SearcherTypeName))
            {
                return ReturnErrorMediaDetails("Please choose Examine Searcher");
            }


            UmbracoHelper helper = new UmbracoHelper(UmbracoContext.Current);

            var mediaDetails = new MediaDetailsDTO();

            var mediaList = new List<MediaDTO>();

            var luceneSearcher = (Examine.LuceneEngine.Providers.LuceneSearcher)ExamineManager.Instance.SearchProviderCollection[search.SearcherTypeName];
            var luceneIndexer = (Examine.LuceneEngine.Providers.LuceneIndexer)ExamineManager.Instance.IndexProviderCollection[luceneSearcher.Name];

            ExamineManager.Instance.IndexProviderCollection["ExternalIndexer"].GatheringNodeData += MediaSearchController_GatheringNodeData;

            var Searcher = ExamineManager.Instance.SearchProviderCollection[search.SearcherTypeName];

            var searchCriteria = Searcher.CreateSearchCriteria(BooleanOperation.And);

            Dictionary<string, string> fieldNames = new Dictionary<string, string>();

            var mediaSearchType = "image";

            // DEFAULT TYPE OF SEARCH
            if (!string.IsNullOrEmpty(search.MediaSearchType))
            {
                List<string> acceptMediaSearch = new List<string>()
                {
                    "image", "file"
                };

                var loweredMediaType = search.MediaSearchType.ToLower().Trim();

                if (acceptMediaSearch.Contains(loweredMediaType))
                {
                    mediaSearchType = loweredMediaType;
                }

            }

            // LIST OF SEARCH FIELD NAMES TO REPLACE WITH UMBRACO FILENAMES
            fieldNames.Add("filename", "nodeName");
            fieldNames.Add("height", "umbracoHeight");
            fieldNames.Add("width", "umbracoWidth");
            fieldNames.Add("filesize", "umbracoBytes");
            fieldNames.Add("fileextension", "umbracoExtension");
            fieldNames.Add("fileext", "umbracoExtension");

            // CREATE QUERY
            var q = searchCriteria.Field("nodeTypeAlias", mediaSearchType);

            // CHECK SEARCH FILTERS
            if (search != null)
            {
                // CHECK FILTERS
                if (search.SearchFilters != null)
                {
                    if (search.SearchFilters.Any())
                    {
                        foreach (var searchFilter in search.SearchFilters)
                        {
                            if (string.IsNullOrEmpty(searchFilter.SearchValue))
                            {
                                continue;
                            }

                            var fieldName = fieldNames[searchFilter.SearchParameter.ToLower()];
                            var fieldValues = searchFilter.SearchValue.Trim().Split(',');
                            var boolOperator = !string.IsNullOrEmpty(searchFilter.BoolOperator) ? searchFilter.BoolOperator.ToUpper() : "";
                            var isRange = searchFilter.IsRange;
                            var rangeOperator = searchFilter.RangeOperator;
                            
                            if (isRange)
                            {
                                double rangeVal = 0;
                                double maxVal = 99000000;
                                double min = 0;

                                double.TryParse(searchFilter.SearchValue, out rangeVal);

                                switch (rangeOperator)
                                {
                                    case "EQUALSTO":
                                        q.And().Field(fieldName, rangeVal.ToString());
                                        break;
                                    case "NOTEQUALSTO":
                                        q.Not().Field(fieldName, rangeVal.ToString());
                                        break;
                                    case "LESSTHAN":
                                        q.And().Range(fieldName, min.ToString(), rangeVal.ToString(), false, false);
                                        break;
                                    case "LESSTHANEQUALSTO":
                                        q.And().Range(fieldName, min.ToString(), rangeVal.ToString());
                                        break;
                                    case "MORETHAN":
                                        q.And().Range(fieldName, rangeVal.ToString(), maxVal.ToString(), false, false);
                                        break;
                                    case "MORETHANEQUALSTO":
                                        q.And().Range(fieldName, rangeVal.ToString(), maxVal.ToString());
                                        break;
                                    default:
                                        q.And().Field(fieldName, rangeVal.ToString());
                                        break;
                                }
                            } else
                            {

                                List<string> tempFieldList = new List<string>();
                                tempFieldList.Add(fieldName);

                                switch (boolOperator)
                                {
                                    case "AND":
                                        q.And().GroupedAnd(tempFieldList, fieldValues);
                                        break;
                                    case "OR":
                                        q.And().GroupedOr(tempFieldList, fieldValues);
                                        break;
                                    case "NOT":
                                        q.And().GroupedNot(tempFieldList, fieldValues);
                                        break;
                                    default:
                                        q.And().GroupedAnd(tempFieldList, fieldValues);
                                        break;
                                }
                            }
                            
                        }
                    }
                }

                string dateStartFormat = "yyyyMMdd000000";
                string dateEndFormat = "yyyyMMdd235959";

                // CHECK DATES
                if (search.CreatedDateStart.HasValue && search.CreatedDateEnd.HasValue)
                {
                    var startDate = search.CreatedDateStart.Value.ToString(dateStartFormat);
                    var endDate = search.CreatedDateEnd.Value.ToString(dateEndFormat);

                    q.And().Range("createDate", startDate, endDate, true, true);

                }

                if (search.ModifiedDateStart.HasValue && search.ModifiedDateEnd.HasValue)
                {
                    var startDate = search.ModifiedDateStart.Value.ToString(dateStartFormat);
                    var endDate = search.ModifiedDateEnd.Value.ToString(dateEndFormat);

                    q.And().Range("updateDate", startDate, endDate, true, true);
                }
                
            }

            var queryComplied = q.Compile();

            IEnumerable<SearchResult> results = Searcher.Search(queryComplied);

            foreach (var result in results)
            {
                var mediaItem = new MediaDTO();

                var typedMedia = helper.TypedMedia(result.Id);

                if (typedMedia == null)
                {
                    continue;
                }

                mediaItem.NodeID = typedMedia.Id;
                mediaItem.Filename = typedMedia.Name;
                mediaItem.Height = typedMedia.GetPropertyValue<int>("umbracoHeight");
                mediaItem.Width = typedMedia.GetPropertyValue<int>("umbracoWidth");
                mediaItem.FileType = typedMedia.GetPropertyValue<string>("__NodeTypeAlias");
                mediaItem.FileExtension = typedMedia.GetPropertyValue<string>("umbracoExtension");
                mediaItem.RelativePath = typedMedia.GetPropertyValue<string>("umbracoFile");
                mediaItem.FullURL = typedMedia.Url;
                mediaItem.Author = typedMedia.WriterName;
                mediaItem.CreatedDate = typedMedia.CreateDate;
                mediaItem.ModifiedDate = typedMedia.UpdateDate;
                mediaItem.FileSize = typedMedia.GetPropertyValue<int>("umbracoBytes");

                mediaList.Add(mediaItem);
            }

            var groupAuthors = string.Empty;

            List<string> authors = new List<string>();

            if (mediaList.Any())
            {
                var authorQuery = (from mediaItem in mediaList
                               group mediaItem.Author by mediaItem.Author into g
                               select new { g.Key }).ToList();

                groupAuthors = string.Join(",", authorQuery.ToList());
                authors = authorQuery.Select(x => x.Key).OrderBy(n => n).ToList();
            }

            mediaDetails.MediaItems = mediaList;
            mediaDetails.Details = new SearchResultsDetails()
            {
                TotalMediaItems = mediaList.Count(),
                NoResultsFound = !mediaList.Any(),
                Query = queryComplied.ToString(),
                ListOfAuthors = authors,
                ErrorMessage = "No Errors Found"
            };

            return mediaDetails;
        }

        private void MediaSearchController_GatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        {
            int maxPadding = 10;

            if (e.Fields.ContainsKey("umbracoBytes"))
            {
                e.Fields["umbracoBytes"] = e.Fields["umbracoBytes"].PadLeft(maxPadding, '0');
            }

            if (e.Fields.ContainsKey("umbracoHeight"))
            {
                e.Fields["umbracoHeight"] = e.Fields["umbracoHeight"].PadLeft(maxPadding, '0');
            }

            if (e.Fields.ContainsKey("umbracoWidth"))
            {
                e.Fields["umbracoWidth"] = e.Fields["umbracoWidth"].PadLeft(maxPadding, '0');
            }
        }


        [AcceptVerbs("POST")]
        [HttpPost]
        public ExamineSearchers AvailableSearchers()
        {
            var searchers = new ExamineSearchers();
            var listOfSearchers = new List<ExamineSearcher>();

            // GET INDEXERS
            var totalIndexers = ExamineManager.Instance.IndexProviderCollection.Count;
            List<ExamineIndexer> listOfIndexers = new List<ExamineIndexer>();

            if (totalIndexers > 0)
            {
                for (var idx = 0; idx < totalIndexers; idx++)
                {
                    var indexObj = (Examine.LuceneEngine.Providers.LuceneIndexer) ExamineManager.Instance.IndexProviderCollection[idx];

                    var indexer = new ExamineIndexer()
                    {
                        Index = idx,
                        Name = indexObj.Name,
                        IndexSetName = indexObj.IndexSetName
                    };

                    listOfIndexers.Add(indexer);
                }

                searchers.Searchers = listOfSearchers.ToList();
            }

            // GET SEARCHERS
            var total = ExamineManager.Instance.SearchProviderCollection.Count;

            searchers.TotalSearchers = total;

            searchers.NoAvailableSearchers = (total == 0);

            if (total > 0)
            {
                for (var idx = 0; idx < total; idx++)
                {
                    var searchIndexer = (Examine.LuceneEngine.Providers.LuceneSearcher) ExamineManager.Instance.SearchProviderCollection[idx];
                    var indexer = listOfIndexers.FirstOrDefault(x => x.IndexSetName == searchIndexer.IndexSetName);

                    var searcher = new ExamineSearcher()
                    {
                        Index = idx,
                        Name = searchIndexer.Name,
                        Description = searchIndexer.Description,
                        IndexerName = (indexer != null) ? indexer.Name : ""
                    };

                    listOfSearchers.Add(searcher);
                }
               
                searchers.Searchers = listOfSearchers.ToList();
            }

            listOfIndexers.Clear();
            listOfIndexers = null;

            return searchers;
        }

        private MediaDetailsDTO ReturnErrorMediaDetails(string errorMessage)
        {
            return new MediaDetailsDTO()
            {
                Details = new SearchResultsDetails()
                {
                    ListOfAuthors = null,
                    ErrorMessage = errorMessage,
                    NoResultsFound = true,
                    Query = string.Empty,
                    TotalMediaItems = 0,
                    IsError = true
                }
            };
        }
    }

}