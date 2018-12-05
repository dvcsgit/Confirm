using DataAccess.ASE;
using DbEntity.ASE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASE.OrganizationHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            var uniqueID = "79bddfd6-8a1b-4742-9f3e-5e77931899c7";

            var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(uniqueID, true);

            using (ASEDbEntities db = new ASEDbEntities())
            {
                db.USERAUTHGROUP.AddRange(db.ACCOUNT.Where(x => organizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.ID).ToList().Select(x => new USERAUTHGROUP
                {
                    USERID = x,
                    AUTHGROUPID = "FAC_General"
                }).ToList());
            }

            //var uniqueID = "1c8932c1-9cdd-4834-8b1b-d975cf3a0cc2";

            //var all = OrganizationDataAccessor.GetDownStreamOrganizationList(uniqueID, true);

            //using (ASEDbEntities db = new ASEDbEntities())
            //{
            //    var parentOrganizationList = db.ORGANIZATION.Where(x => x.PARENTUNIQUEID == uniqueID).ToList();

            //    foreach (var parentOrganization in parentOrganizationList)
            //    {
            //        var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(parentOrganization.UNIQUEID, true);

            //        foreach (var downStreamOrganization in downStreamOrganizationList)
            //        {
            //            var temp = OrganizationDataAccessor.GetDownStreamOrganizationList(downStreamOrganization, true);

            //            foreach (var a in all)
            //            {
            //                if (!temp.Contains(a))
            //                {
            //                    db.QUERYABLEORGANIZATION.Add(new QUERYABLEORGANIZATION()
            //                    {
            //                        ORGANIZATIONUNIQUEID = downStreamOrganization,
            //                        QUERYABLEORGANIZATIONUNIQUEID = a
            //                    });
            //                }
            //            }

            //            foreach (var d in downStreamOrganizationList)
            //            {
            //                if (!temp.Contains(d))
            //                {
            //                    db.EDITABLEORGANIZATION.Add(new EDITABLEORGANIZATION()
            //                    {
            //                        ORGANIZATIONUNIQUEID = downStreamOrganization,
            //                        EDITABLEORGANIZATIONUNIQUEID = d
            //                    });
            //                }
            //            }

            //            db.SaveChanges();
            //        }
            //    }
            //}
        }
    }
}
