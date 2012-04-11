using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;
using System.Collections.ObjectModel;
using Raven.Client;

namespace NServiceBus.Management.Errors.DataAccess.Query
{
    public class QueryFromRavenDB : IQueryErrorPersistence
    {
        private readonly static QueryFromRavenDB instance = new QueryFromRavenDB();
        private IDocumentStore documentStore;
        public static QueryFromRavenDB Instance { get { return instance; } }

        private QueryFromRavenDB() { }

        public IDocumentStore DocumentStore
        {
            get { return documentStore; }
            set { documentStore = value; }
        }

        #region IQueryErrorPersistence Members

        public ReadOnlyCollection<IErrorMessageDetails> ErrorMessages
        {
            get 
            {
                using (var session = documentStore.OpenSession())
                {
                    var errorMessages = from errMsg in session.Query<ErrorMessageReceived>()
                                        orderby errMsg.ErrorReceivedTime
                                        select errMsg;
                    return errorMessages.ToList<IErrorMessageDetails>().AsReadOnly();
                }
            }
        }

        #endregion
    }
}
