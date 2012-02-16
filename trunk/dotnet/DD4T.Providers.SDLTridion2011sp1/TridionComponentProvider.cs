﻿using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using T = Tridion.ContentDelivery.DynamicContent;
using Tridion.ContentDelivery.DynamicContent.Query;
using Query = Tridion.ContentDelivery.DynamicContent.Query.Query;
using TMeta = Tridion.ContentDelivery.Meta;
using Tridion.ContentDelivery.Web.Linking;

using DD4T.ContentModel;
using DD4T.ContentModel.Exceptions;
using DD4T.ContentModel.Factories;
using System.Collections.Generic;

using System.Web.Caching;
using System.Web;
using DD4T.ContentModel.Contracts.Providers;
using System.Collections;
using System.Configuration;
using DD4T.ContentModel.Querying;
using DD4T.Utils;

namespace DD4T.Providers.SDLTridion2011sp1
{
    /// <summary>
    /// 
    /// </summary>
    public class TridionComponentProvider : BaseProvider, IComponentProvider
    {


        Dictionary<int,T.ComponentPresentationFactory> _cpFactoryList = null;
        Dictionary<int,TMeta.ComponentMetaFactory> _cmFactoryList = null;

        private string selectByComponentTemplateId;
        private string selectByOutputFormat;

        public TridionComponentProvider()
        {
            selectByComponentTemplateId = ConfigurationManager.AppSettings["ComponentFactory.ComponentTemplateId"];
            selectByOutputFormat = ConfigurationManager.AppSettings["ComponentFactory.OutputFormat"];
            _cpFactoryList = new Dictionary<int,T.ComponentPresentationFactory>();
            _cmFactoryList = new Dictionary<int,TMeta.ComponentMetaFactory>();
        }

        #region IComponentProvider
        public string GetContent(string uri)
        {
            SiteLogger.Debug(">>GetContent({0})", LoggingCategory.Performance, uri);

            TcmUri tcmUri = new TcmUri(uri);

            T.ComponentPresentationFactory cpFactory = GetComponentPresentationFactory(tcmUri.PublicationId);

            T.ComponentPresentation cp = null;


            if (!string.IsNullOrEmpty(selectByComponentTemplateId))
            {
                cp = cpFactory.GetComponentPresentation(tcmUri.ItemId, Convert.ToInt32(selectByComponentTemplateId));
                if (cp != null)
                {
                    SiteLogger.Debug("<<GetContent({0}) - by ct id", LoggingCategory.Performance, uri);
                    return cp.Content;
                }
            }
            if (!string.IsNullOrEmpty(selectByOutputFormat))
            {
                cp = cpFactory.GetComponentPresentationWithOutputFormat(tcmUri.ItemId, selectByOutputFormat);
                if (cp != null)
                {
                    SiteLogger.Debug("<<GetContent({0}) - by output format", LoggingCategory.Performance, uri);
                    return cp.Content;
                }
            }
            SiteLogger.Debug("GetContent: about to find all component presentations for {0}", LoggingCategory.Performance, tcmUri.ToString());
            IList cps = cpFactory.FindAllComponentPresentations(tcmUri.ItemId);
            SiteLogger.Debug("GetContent: found all component presentations for {0}", LoggingCategory.Performance, tcmUri.ToString());

            foreach (Tridion.ContentDelivery.DynamicContent.ComponentPresentation _cp in cps)
            {
                if (_cp != null)
                {
                    if (_cp.Content.Contains("<Component"))
                    {
                        SiteLogger.Debug("<<GetContent({0}) - find all", LoggingCategory.Performance, uri);
                        return _cp.Content;
                    }
                }
            }
            SiteLogger.Debug("<<GetContent({0}) - not found", LoggingCategory.Performance, uri);
            return string.Empty;
        }

        /// <summary>
        /// Returns the Component contents which could be found. Components that couldn't be found don't appear in the list. 
        /// </summary>
        /// <param name="componentUris"></param>
        /// <returns></returns>
        public List<string> GetContentMultiple(string[] componentUris)
        {
//            TcmUri uri = new TcmUri(componentUris.First());
            var components =
                componentUris
                .Select(componentUri => { TcmUri uri = new TcmUri(componentUri); return (T.ComponentPresentation)GetComponentPresentationFactory(uri.PublicationId).FindAllComponentPresentations(componentUri)[0]; })
                .Where(cp => cp != null)
                .Select(cp => cp.Content)
                .ToList();

            return components;

        }
         
        public IList<string> FindComponents(IQuery query)
        {
            if (! (query is ITridionQueryWrapper))
                throw new InvalidCastException("Cannot execute query because it is not based on " + typeof(ITridionQueryWrapper).Name);

            Query tridionQuery = ((ITridionQueryWrapper)query).ToTridionQuery();
            return tridionQuery.ExecuteQuery();
        }


        public DateTime GetLastPublishedDate(string uri)
        {
            TcmUri tcmUri = new TcmUri(uri);
            TMeta.IComponentMeta cmeta = GetComponentMetaFactory(tcmUri.PublicationId).GetMeta(tcmUri.ItemId);
            return cmeta == null ? DateTime.Now : cmeta.LastPublicationDate;
        }
        #endregion

        #region private
        private object lock1 = new object();
        private object lock2 = new object();
        private TMeta.ComponentMetaFactory GetComponentMetaFactory(int publicationId)
        {
            if (_cmFactoryList.ContainsKey(publicationId))
                return _cmFactoryList[publicationId];

            lock (lock1)
            {
                if (!_cmFactoryList.ContainsKey(publicationId)) // we must test again, because in the mean time another thread might have added a record to the dictionary!
                {
                    _cmFactoryList.Add(publicationId, new TMeta.ComponentMetaFactory(publicationId)); 
                }
            }
            return _cmFactoryList[publicationId];
        }
        private T.ComponentPresentationFactory GetComponentPresentationFactory(int publicationId)
        {
            if (_cpFactoryList.ContainsKey(publicationId))
                return _cpFactoryList[publicationId];

            lock (lock2)
            {
                if (!_cpFactoryList.ContainsKey(publicationId)) // we must test again, because in the mean time another thread might have added a record to the dictionary!
                {
                    _cpFactoryList.Add(publicationId, new T.ComponentPresentationFactory(publicationId));
                }
            }
            return _cpFactoryList[publicationId];
        }
        #endregion
    }
}