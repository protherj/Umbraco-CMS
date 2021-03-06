﻿/**
    * @ngdoc service 
    * @name umbraco.resources.sectionResource     
    * @description Loads in data for section
    **/
function legacyResource($q, $http, umbRequestHelper) {

   
    //the factory object returned
    return {
        /** Loads in the data to display the section list */
        deleteItem: function (args) {
            
            if (!args.nodeId || !args.nodeType) {
                throw "The args parameter is not formatted correct, it requires properties: nodeId, nodeType";
            } 

            return umbRequestHelper.resourcePromise(
                $http.delete(
                    umbRequestHelper.getApiUrl(
                        "legacyApiBaseUrl",
                        "DeleteLegacyItem",
                        [{ nodeId: args.nodeId }, { nodeType: args.nodeType }])),
                'Failed to delete item ' + args.nodeId);

        }
    };
}

angular.module('umbraco.resources').factory('legacyResource', legacyResource);