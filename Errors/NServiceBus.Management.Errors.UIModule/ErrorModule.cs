using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;

namespace NServiceBus.Management.Errors.UIModule
{
    public class ErrorModule : IModule
    {
        private readonly IRegionManager regionManager;

        public ErrorModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void Initialize()
        {
            regionManager.RegisterViewWithRegion("MainRegion", typeof(View.ErrorMessageDetailsView));
        }
    }
}
