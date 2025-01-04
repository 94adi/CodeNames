namespace CodeNames.Components;

public class ModalViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string id, string title, string body)
    {
        ViewData["ModalId"] = id;
        ViewData["ModalTitle"] = title;
        ViewData["ModalBody"] = body;
        return View();
    }
}
