
namespace Brigade.Models
{
    public class WorkflowType : EntityBase<Brigade>
	{
		public string WorkflowTypeId { get; set; }
		public string Description { get; set; }
		public string WorkflowDefinitionJson { get; set; }
	}
}
