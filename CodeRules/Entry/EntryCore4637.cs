﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine;
    

    /// <summary>
    /// Class of entension rule for Entry.Core.4637
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4637 : ExtensionRule
    {
        /// <summary>
        /// Gets Category property
        /// </summary>
        public override string Category
        {
            get
            {
                return "core";
            }
        }

        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Entry.Core.4637";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The URL of navigation property should be the canonical URL for the navigation property.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "8.1.1.2";
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V3_V4;
            }
        }

        /// <summary>
        /// Gets location of help information of the rule
        /// </summary>
        public override string HelpLink
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the error message for validation failure
        /// </summary>
        public override string ErrorMessage
        {
            get
            {
                return this.Description;
            }
        }

        /// <summary>
        /// Gets the requriement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Should;
            }
        }

        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Entry;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Atom;
            }
        }

        /// <summary>
        /// Gets the RequireMetadata property to which the rule applies.
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the IsOfflineContext property to which the rule applies.
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Verify Entry.Core.4637
        /// </summary>
        /// <param name="context">Service context</param>
        /// <param name="info">out paramater to return violation information when rule fail</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;

            string xpath = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty']", context.EntityTypeShortName);
            XElement metadata = XElement.Parse(context.MetadataDocument);
            var navigProps = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            List<string> navigPropCanonicalURLs = new List<string>();

            foreach (var np in navigProps)
            {
                navigPropCanonicalURLs.Add(context.DestinationBaseLastSegment + @"/" + np.Attribute(@"Name").Value);
            }
            
            xpath = @"./atom:link[@type='application/atom+xml;type=entry' or @type='application/atom+xml;type=feed']";
            XElement payload = XElement.Parse(context.ResponsePayload);
            var linkElements = payload.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            foreach (XElement le in linkElements)
            {
                if (navigPropCanonicalURLs.Contains(le.Attribute("href").Value))
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                    break;
                }
            }

            return passed;
        }
    }
}