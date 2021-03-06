/**
    * @ngdoc service 
    * @name umbraco.resources.contentResource
    * @description Loads/saves in data for content
    **/
function contentResource($q, $http, umbDataFormatter, umbRequestHelper) {

    /** internal method process the saving of data and post processing the result */
    function saveContentItem(content, action, files) {
        return umbRequestHelper.postSaveContent(
            umbRequestHelper.getApiUrl(
                "contentApiBaseUrl",
                "PostSave"),
            content, action, files);
    }

    return {
        
        deleteById: function(id) {
            return umbRequestHelper.resourcePromise(
                $http.delete(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "DeleteById",
                        [{ id: id }])),
                'Failed to delete item ' + id);
        },

        getById: function (id) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentApiBaseUrl",
                       "GetById",
                       [{ id: id }])),
               'Failed to retreive data for content id ' + id);
        },
        
        getByIds: function (ids) {
            
            var idQuery = "";
            _.each(ids, function(item) {
                idQuery += "ids=" + item + "&";
            });

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentApiBaseUrl",
                       "GetByIds",
                       idQuery)),
               'Failed to retreive data for content id ' + id);
        },

        /** returns an empty content object which can be persistent on the content service
            requires the parent id and the alias of the content type to base the scaffold on */
        getScaffold: function (parentId, alias) {
            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentApiBaseUrl",
                       "GetEmpty",
                       [{ contentTypeAlias: alias }, { parentId: parentId }])),
               'Failed to retreive data for empty content item type ' + alias);
        },

        getChildren: function (parentId, options) {

            //TODO: Make this real

            if (options === undefined) {
                options = {
                    take: 10,
                    offset: 0,
                    filter: ''
                };
            }

            var collection = { take: 10, total: 68, pages: 7, currentPage: options.offset, filter: options.filter };
            collection.total = 56 - (options.filter.length);
            collection.pages = Math.round(collection.total / collection.take);
            collection.resultSet = [];

            if (collection.total < options.take) {
                collection.take = collection.total;
            } else {
                collection.take = options.take;
            }


            var _id = 0;
            for (var i = 0; i < collection.take; i++) {
                _id = (parentId + i) * options.offset;
                var cnt = this.getById(_id);

                //here we fake filtering
                if (options.filter !== '') {
                    cnt.name = options.filter + cnt.name;
                }

                collection.resultSet.push(cnt);
            }

            return collection;
        },

        /** saves or updates a content object */
        saveContent: function (content, isNew, files) {
            return saveContentItem(content, "save" + (isNew ? "New" : ""), files);
        },

        /** saves and publishes a content object */
        publishContent: function (content, isNew, files) {
            return saveContentItem(content, "publish" + (isNew ? "New" : ""), files);
        }

    };
}

angular.module('umbraco.resources').factory('contentResource', contentResource);
