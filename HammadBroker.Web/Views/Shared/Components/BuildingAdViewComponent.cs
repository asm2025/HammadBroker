using HammadBroker.Model.DTO;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace HammadBroker.Web.Views.Shared.Components;

public class BuildingAdViewComponent : ViewComponent
{
    public BuildingAdViewComponent()
    {
    }

    [NotNull]
    public IViewComponentResult Invoke([NotNull] BuildingAdModel model)
    {
        return View(model);
    }
}