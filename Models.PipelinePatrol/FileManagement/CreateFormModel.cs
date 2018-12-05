namespace Models.PipelinePatrol.FileManagement
{
    public class CreateFormModel
    {
        public string OrganizationUniqueID { get; set; }

        public string PipelineUniqueID { get; set; }

        public string PipePointUniqueID { get; set; }

        public string FolderUniqueID { get; set; }

        public FormInput FormInput { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
        }
    }
}
