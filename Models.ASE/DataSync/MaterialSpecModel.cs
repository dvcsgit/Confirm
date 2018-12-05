namespace Models.ASE.DataSync
{
   public class MaterialSpecModel
   {
       public string MaterialUniqueID { get; set; }

       public string Spec { get; set; }

       public string Option { get; set; }

       public string Input { get; set; }

       public string Value
       {
           get
           {
               if (!string.IsNullOrEmpty(Option))
               {
                   return Option;
               }
               else
               {
                   return Input;
               }
           }
       }

       public override bool Equals(object Object)
       {
           return Equals(Object as MaterialSpecModel);
       }

       public override int GetHashCode()
       {
           return MaterialUniqueID.GetHashCode() + Spec.GetHashCode();
       }

       public bool Equals(MaterialSpecModel Model)
       {
           return MaterialUniqueID.Equals(Model.MaterialUniqueID) && Spec.Equals(Model.Spec);
       }
    }
}
