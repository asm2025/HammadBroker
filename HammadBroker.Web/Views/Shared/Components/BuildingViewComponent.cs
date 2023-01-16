using HammadBroker.Model.DTO;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace HammadBroker.Web.Views.Shared.Components;

public class BuildingViewComponent : ViewComponent
{
    public BuildingViewComponent()
    {
    }

    [NotNull]
    public IViewComponentResult Invoke([NotNull] BuildingModel model)
    {
        return View(model);
    }
}