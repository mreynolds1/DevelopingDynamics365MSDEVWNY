using CRMExtensions.ORM;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CRMExtensions
{
    public class RSVPPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.  
            // If you are not registering the plug-in in the sandbox, then you do  
            // not have to add any tracing service related code.  
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            tracingService.Trace("In RSVPPlugin");


            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Get the entity being operated om
                Entity entity = (Entity)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    tracingService.Trace("Running for entity {0}", entity.LogicalName);

                    // Should only run for the RSVP entity 
                    if (entity.LogicalName != CRMExtensions.ORM.msdev_rsvp.EntityLogicalName)
                        throw new InvalidPluginExecutionException("Unsupported registration");


                    Dynamics365Context svc = new Dynamics365Context(service);

                    var rsvp = entity.ToEntity<msdev_rsvp>();

                    // Need to ensure we do not exceed max attendance. If this RSVP is not attending,
                    // no need to check
                    if (rsvp.msdev_Attending.GetValueOrDefault(false))
                    {
                        // Get max attendance for the event and compare against number of attending RSVPs
                        var rsvpEvent = svc.msdev_meetupSet.First(e => e.msdev_meetupId == rsvp.msdev_MeetupId.Id);

                        var eventAttendees = svc.msdev_rsvpSet.Where(
                            r => r.msdev_MeetupId != null
                            && r.msdev_MeetupId.Id == rsvpEvent.Id
                            && r.msdev_Attending == true)
                            .ToList();

                        if ((eventAttendees.Count + 1) > rsvpEvent.msdev_MaxAttendees.GetValueOrDefault(Int32.MaxValue))
                        {
                            throw new InvalidPluginExecutionException("Maximum attendance exceeded");
                        }

                    }

                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in MyPlug-in.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("MyPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
