using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;
using Raven.Client.Document;
using Raven.Client;

namespace NServiceBus.Management.Errors.DataAccess.Persister
{
    public class PersistErrorsInRavenDB : IPersistErrorMessages
    {
        private readonly static PersistErrorsInRavenDB instance = new PersistErrorsInRavenDB();
        private IDocumentStore documentStore;
        public static PersistErrorsInRavenDB Instance { get { return instance; } }

        private PersistErrorsInRavenDB()
        {
        }

        public IDocumentStore DocumentStore
        {
            get { return documentStore; }
            set { documentStore = value; }
        }

        #region IPersistErrorMessages Members

        public void SaveErrorMessage(IErrorMessageDetails details)
        {
            using (var session = documentStore.OpenSession())
            {
                string id = details.FailedMessageId;
                session.Store(details);
                session.SaveChanges();
            }
        }

        public void DeleteErrorMessage(string messageId)
        {
            using (var session = documentStore.OpenSession())
            {
                // The following Linq query does not work!!!!! Looks like a bug in the linq provider.
                //var errorMessage = (from errMsg in session.Query<ErrorMessageReceived>()
                //                    where (errMsg.FailedMessageId.Equals(messageId) || errMsg.OriginalMessageId.Equals(messageId))
                //                    select errMsg).FirstOrDefault();

                var messages = from errMsg in session.Query<ErrorMessageReceived>()
                               select errMsg;

                ErrorMessageReceived errorMessage = null;
                foreach (ErrorMessageReceived errMsg in messages)
                {
                    if (errMsg.FailedMessageId.Equals(messageId) || errMsg.OriginalMessageId.Equals(messageId))
                    {
                        errorMessage = errMsg;
                        break;
                    }
                }

                //if (errorMessage != null)
                {
                    session.Delete<ErrorMessageReceived>(errorMessage);
                    session.SaveChanges();
                }
            }
        }

        #endregion
    }
}
