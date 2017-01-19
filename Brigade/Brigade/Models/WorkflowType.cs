
namespace Brigade.Models
{
    public class WorkflowType : AnyContainer
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string WorkflowDefinitionJson { get; set; }
	}
}
