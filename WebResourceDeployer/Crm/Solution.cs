﻿using CrmDeveloperExtensions2.Core;
using CrmDeveloperExtensions2.Core.Enums;
using CrmDeveloperExtensions2.Core.Logging;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using NLog;
using System;
using WebResourceDeployer.Resources;

namespace WebResourceDeployer.Crm
{
    public static class Solution
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static EntityCollection RetrieveSolutionsFromCrm(CrmServiceClient client, bool getManaged)
        {
            try
            {
                QueryExpression query = new QueryExpression
                {
                    EntityName = "solution",
                    ColumnSet = new ColumnSet("friendlyname", "solutionid", "uniquename"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression
                            {
                                AttributeName = "isvisible",
                                Operator = ConditionOperator.Equal,
                                Values = {true}
                            }
                        }
                    },
                    LinkEntities =
                    {
                        new LinkEntity
                        {
                            LinkFromEntityName = "solution",
                            LinkFromAttributeName = "publisherid",
                            LinkToEntityName = "publisher",
                            LinkToAttributeName = "publisherid",
                            Columns = new ColumnSet("customizationprefix"),
                            EntityAlias = "publisher"
                        }
                    },
                    Distinct = true,
                    Orders =
                    {
                        new OrderExpression
                        {
                            AttributeName = "friendlyname",
                            OrderType = OrderType.Ascending
                        }
                    }
                };

                if (!getManaged)
                {
                    ConditionExpression noManaged = new ConditionExpression
                    {
                        AttributeName = "ismanaged",
                        Operator = ConditionOperator.Equal,
                        Values = { false }
                    };

                    query.Criteria.Conditions.Add(noManaged);
                }

                EntityCollection solutions = client.RetrieveMultiple(query);

                OutputLogger.WriteToOutputWindow(Resource.Message_RetrievedSolutions, MessageType.Info);

                return solutions;
            }
            catch (Exception ex)
            {
                ExceptionHandler.LogException(Logger, Resource.ErrorMessage_ErrorRetrievingSolutions, ex);

                return null;
            }
        }

        public static bool AddWebResourceToSolution(CrmServiceClient client, string uniqueName, Guid webResourceId)
        {
            try
            {
                AddSolutionComponentRequest scRequest = new AddSolutionComponentRequest
                {
                    ComponentType = 61,
                    SolutionUniqueName = uniqueName,
                    ComponentId = webResourceId
                };

                client.Execute(scRequest);

                OutputLogger.WriteToOutputWindow(Resource.Message_NewWebResourceAddedSolution, MessageType.Info);

                return true;
            }
            catch (Exception ex)
            {
                ExceptionHandler.LogException(Logger, Resource.ErrorMessage_ErrorAddingWebResourceSolution, ex);

                return false;
            }
        }
    }
}