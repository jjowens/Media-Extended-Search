angular.module("umbraco")
    .controller("My.MediaExtendedSearchController",
        function ($scope, $http) {
            $scope.AvailableSearchers = [];
            $scope.NoAvailableSearchers = false;
            $scope.TotalSearchers = 0;
            $scope.mediaSearcherType = "";
            $scope.mediaSearcherTypeSelected = "";
            $scope.ListOfAuthors = [];
            $scope.ListOfFileTypes = [];

            $scope.MediaTypes = [];

            $scope.SearchResults = [];
            $scope.FilteredResults = [];

            $scope.TotalSearchResults = 0;

            $scope.mediaSearchRangeOperators = [
                { value: "LESSTHAN", name: "Less Than" },
                { value: "LESSTHANEQUALSTO", name: "Less Than and Equals To" },
                { value: "EQUALSTO", name: "Equals To" },
                { value: "MORETHAN", name: "More Than" },
                { value: "MORETHANEQUALSTO", name: "More Than and Equals To" },
                { value: "NOTEQUALSTO", name: "Not Equals To" }
            ];

            $scope.mediaSearchFileName = "";
            $scope.mediaSearchMediaType = "image";
            $scope.mediaSearchWidth = "";
            $scope.mediaSearchHeight = "";
            $scope.mediaSearchFileExtension = "";
            $scope.mediaSearchFileSize = "";
            $scope.mediaSearchImageDisabled = false;

            $scope.mediaSearchDateCreatedStart = CreateDatePicker();
            $scope.mediaSearchDateCreatedEnd = CreateDatePicker();
            $scope.mediaSearchDateModifiedStart = CreateDatePicker();
            $scope.mediaSearchDateModifiedEnd = CreateDatePicker();

            $scope.NoResultsFound = false;

            $scope.SortByFieldName = "";
            $scope.SortByFieldOrder = "ASC";
            $scope.OrderByHeader = "Filename";
            $scope.ReverseSort = false;

            $scope.mediaSearchWidthRange = "EQUALSTO";
            $scope.mediaSearchHeightRange = "EQUALSTO";
            $scope.mediaSearchFileSizeRange = "EQUALSTO";

            $scope.mediaSearchMediaChange = function () {
                var toggleDisabled = true;

                if ($scope.mediaSearchMediaType == "image") {
                    toggleDisabled = false;
                }

                $scope.mediaSearchImageDisabled = toggleDisabled;
            };

            $scope.SearchMedia = function () {
                // SEND TO SERVICE
                var url = "/umbraco/backoffice/MediaExtendedSearchService/MediaSearch/SearchExamine";

                var SearchFilters = [];

                SearchFilters.push(createSearchFilter("Filename", $scope.mediaSearchFileName, "OR", false, "", false));
                SearchFilters.push(createSearchFilter("FileSize", $scope.mediaSearchFileSize, "AND", true, $scope.mediaSearchFileSizeRange, false));
                SearchFilters.push(createSearchFilter("FileExtension", $scope.mediaSearchFileExtension, "OR", false, "", false));

                // SEARCH FOR DIMENSIONS ONLY IF IMAGE IS SELECTED
                if ($scope.mediaSearchMediaType == "image") {
                    SearchFilters.push(createSearchFilter("Width", $scope.mediaSearchWidth, "AND", true, $scope.mediaSearchWidthRange, false));
                    SearchFilters.push(createSearchFilter("Height", $scope.mediaSearchHeight, "AND", true, $scope.mediaSearchHeightRange, false));
                }

                var data = {
                    "SearchFilters": SearchFilters,
                    "MediaSearchType": $scope.mediaSearchMediaType,
                    "CreatedDateStart": $scope.mediaSearchDateCreatedStart.value,
                    "CreatedDateEnd": $scope.mediaSearchDateCreatedEnd.value,
                    "ModifiedDateStart": $scope.mediaSearchDateModifiedStart.value,
                    "ModifiedDateEnd": $scope.mediaSearchDateModifiedEnd.value,
                    "SearcherTypeName": $scope.mediaSearcherType
                };

                $http.post(url, JSON.stringify(data)).then(function (response) {
                    console.log(response);

                    $scope.SearchResults = [];
                    $scope.NoResultsFound = false;

                    $scope.SearchResults = response.data.MediaItems;
                    $scope.NoResultsFound = response.data.Details.NoResultsFound;
                    $scope.TotalSearchResults = (response.data.Details.NoResultsFound) ? 0 : response.data.MediaItems.length;
                    $scope.ListOfAuthors = response.data.ListOfAuthors;
                    $scope.IsError = response.data.Details.IsError;
                    $scope.ErrorMessage = response.data.Details.ErrorMessage;


                }, function (response) {

                });

            };

            function createSearchFilter(searchParameter, searchValue, boolOperator, isRange, RangeOperator, isWildCard) {
                var searchFilter = {
                    "SearchParameter": searchParameter,
                    "SearchValue": searchValue,
                    "BoolOperator": boolOperator,
                    "IsRange": isRange,
                    "RangeOperator": RangeOperator,
                    "IsWildCard": isWildCard
                };

                return searchFilter;
            }

            $scope.ClearSearch = function () {

                $scope.mediaSearchFileName = "";
                $scope.mediaSearchMediaType = "image";
                $scope.mediaSearchWidth = "";
                $scope.mediaSearchHeight = "";
                $scope.mediaSearchFileExtension = "";
                $scope.mediaSearchFileSize = "";
                $scope.mediaSearchImageDisabled = false;

                $scope.SortByFieldOrder = "ASC";
                $scope.OrderByHeader = "Filename";

                $scope.mediaSearchWidthRange = "EQUALSTO";
                $scope.mediaSearchHeightRange = "EQUALSTO";
                $scope.mediaSearchFileSizeRange = "EQUALSTO";

                $scope.SearchResults = [];
                $scope.TotalSearchResults = 0;

                console.log("Cleared Search 02");
            }

            $scope.SortByHeader = function (event) {
                var header = event.target.dataset.headername;

                var reverseSort = false;

                if ($scope.OrderByHeader == header) {
                    reverseSort = !$scope.ReverseSort;
                }

                var sortIcons = document.getElementsByClassName("sort-by-icon");

                for (var i = 0; i < sortIcons.length; i++) {
                    sortIcons[i].innerHTML = "";
                };

                var headerSortIcon = event.target.getElementsByClassName("sort-by-icon");
                headerSortIcon[0].innerHTML = (reverseSort) ? "&darr;" : "&uarr;";

                $scope.OrderByHeader = header;
                $scope.ReverseSort = reverseSort;
            }

            function CreateDatePicker() {
                return {
                    editor: "Umbraco.DateTime",
                    alias: "datepicker",
                    view: 'datepicker',
                    value: null,
                    config: {
                        pickDate: true,
                        pickTime: false,
                        useSeconds: false,
                        format: "YYYY-MM-DD",
                        icons: {
                            time: "icon-time",
                            date: "icon-calendar",
                            up: "icon-chevron-up",
                            down: "icon-chevron-down"
                        }
                    }
                };
            };

            function UpdateDatePicker(dateVal) {
                console.log("Update DatePicker");

                return {
                    editor: "Umbraco.DateTime",
                    alias: "datepicker",
                    view: 'datepicker',
                    value: null,
                    config: {
                        pickDate: true,
                        pickTime: false,
                        useSeconds: false,
                        format: "YYYY-MM-DD",
                        icons: {
                            time: "icon-time",
                            date: "icon-calendar",
                            up: "icon-chevron-up",
                            down: "icon-chevron-down"
                        },
                        value: dateVal
                    }
                };
            };

            function CreateDatePickerWithID(id) {
                return {
                    editor: "Umbraco.DateTime",
                    alias: "id",
                    view: 'datepicker',
                    value: null,
                    config: {
                        pickDate: true,
                        pickTime: false,
                        useSeconds: false,
                        format: "YYYY-MM-DD",
                        icons: {
                            time: "icon-time",
                            date: "icon-calendar",
                            up: "icon-chevron-up",
                            down: "icon-chevron-down"
                        }
                    }
                };
            };

            $scope.formatBytes = function (bytes) {
                var decimals = 0;
                if (bytes === 0) return '';

                const k = 1024;
                const dm = decimals < 0 ? 0 : decimals;
                const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

                const i = Math.floor(Math.log(bytes) / Math.log(k));

                return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
            };

            $scope.GetAvailableSearchers = function () {
                // SEND TO SERVICE
                var url = "/umbraco/backoffice/MediaExtendedSearchService/MediaSearch/AvailableSearchers";

                $http.post(url).then(function (response) {

                    $scope.AvailableSearchers = [];
                    $scope.NoAvailableSearchers = false;

                    $scope.AvailableSearchers = response.data.Searchers;
                    $scope.NoAvailableSearchers = response.data.NoAvailableSearchers;
                    $scope.TotalSearchers = response.data.TotalSearchers;

                    if ($scope.AvailableSearchers.length > 0) {
                        $scope.mediaSearcherType = response.data.Searchers[0].Name;
                    }

                    console.log($scope.AvailableSearchers.length);
                    console.log($scope.AvailableSearchers);
                    console.log($scope.mediaSearcherType);
                    console.log(response.data.Searchers[0]);
                    console.log(response.data.Searchers[0].Name);

                }, function (response) {

                });

            };

            $scope.formatDate = function (dateVal) {
                var newDate = new Date(dateVal);

                finalDate = newDate.getFullYear() + "-" + (pad(newDate.getMonth() + 1)) + "-" + pad(newDate.getDate()) + " " + pad(newDate.getHours()) + ":" + pad(newDate.getMinutes()) + ":" + pad(newDate.getSeconds());

                return finalDate;
            };

            function pad(n) {
                return n < 10 ? '0' + n : n;
            }

            // DO SOMETHING ON LOAD
            $scope.GetAvailableSearchers();

    });