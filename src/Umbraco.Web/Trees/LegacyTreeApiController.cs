﻿using System;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Trees
{

    /// <summary>
    /// This is used to output JSON from legacy trees
    /// </summary>
    [PluginController("UmbracoTrees")]
    public class LegacyTreeApiController : UmbracoAuthorizedApiController
    {
        /// <summary>
        /// Convert a legacy tree to a new tree result
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        [HttpQueryStringFilter("queryStrings")]
        public TreeNodeCollection GetNodes(string id, FormDataCollection queryStrings)
        {
            var tree = GetTree(queryStrings);
            var attempt = tree.TryLoadFromLegacyTree(id, queryStrings, Url, tree.ApplicationAlias);
            if (attempt.Success == false)
            {
                var msg = "Could not render tree " + queryStrings.GetRequiredString("treeType") + " for node id " + id;
                LogHelper.Error<LegacyTreeApiController>(msg, attempt.Error);
                throw new ApplicationException(msg);
            }

            return attempt.Result;
        }

        /// <summary>
        /// This will return the menu item collection for the tree node with the specified Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        /// <remarks>
        /// Due to the nature of legacy trees this means that we need to lookup the parent node, render
        /// the TreeNodeCollection and then find the node we're looking for and render it's menu.
        /// </remarks>
        [HttpQueryStringFilter("queryStrings")]
        public MenuItemCollection GetMenu(string id, FormDataCollection queryStrings)
        {            
            //get the parent id from the query strings
            var parentId = queryStrings.GetRequiredString("parentId");
            var tree = GetTree(queryStrings);

            var rootIds = new[]
            {
                Core.Constants.System.Root.ToString(CultureInfo.InvariantCulture),
                Core.Constants.System.RecycleBinContent.ToString(CultureInfo.InvariantCulture),
                Core.Constants.System.RecycleBinMedia.ToString(CultureInfo.InvariantCulture)
            };

            //if the id and the parentId are both -1 then we need to get the menu for the root node
            if (rootIds.Contains(id) && parentId == "-1")
            {
                var attempt = tree.TryGetMenuFromLegacyTreeRootNode(queryStrings, Url);
                if (attempt.Success == false)
                {
                    var msg = "Could not render menu for root node for treeType " + queryStrings.GetRequiredString("treeType");
                    LogHelper.Error<LegacyTreeApiController>(msg, attempt.Error);
                    throw new ApplicationException(msg);
                }
                return attempt.Result;
            }
            else
            {
                var attempt = tree.TryGetMenuFromLegacyTreeNode(parentId, id, queryStrings, Url);
                if (attempt.Success == false)
                {
                    var msg = "Could not render menu for treeType " + queryStrings.GetRequiredString("treeType") + " for node id " + parentId;
                    LogHelper.Error<LegacyTreeApiController>(msg, attempt.Error);
                    throw new ApplicationException(msg);
                }
                return attempt.Result;
            }                       
        }

        private ApplicationTree GetTree(FormDataCollection queryStrings)
        {
            //need to ensure we have a tree type
            var treeType = queryStrings.GetRequiredString("treeType");
            //now we'll look up that tree
            var tree = Services.ApplicationTreeService.GetByAlias(treeType);
            if (tree == null)
                throw new InvalidOperationException("No tree found with alias " + treeType);
            return tree;
        }

    }
}