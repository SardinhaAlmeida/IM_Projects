using Microsoft.Office.Interop.PowerPoint;

var pptApp = new Application();
var presentation = pptApp.Presentations.Open(@"IM First Presentation.pptx");
presentation.SlideShowSettings.Run();
