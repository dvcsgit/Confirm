using DbEntity.MSSQL.PipelinePatrol;
using Models.Authenticated;
using Models.PipelinePatrol.PipelineManagement;
using System;
using System.Reflection;
using Utility;
using Utility.Models;
using System.Linq;
using Models.PipelinePatrol.Shared;
using System.Collections.Generic;
using Models.Shared;
using DbEntity.MSSQL;
using System.Transactions;

namespace DataAccess.PipelinePatrol
{
    public class PipelineDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.Pipeline.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    result.ReturnData(new GridViewModel()
                    {
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            ID = x.ID
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetDetailViewModel(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var pipeline = db.Pipeline.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = pipeline.UniqueID,
                        Permission = Account.OrganizationPermission(pipeline.OrganizationUniqueID),
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(pipeline.OrganizationUniqueID),
                        ID = pipeline.ID,
                        Color = pipeline.Color,
                        Locus = db.PipelineLocus.Where(x => x.PipelineUniqueID == pipeline.UniqueID).OrderBy(x => x.Seq).Select(x => new Location
                        {
                            LAT = x.LAT,
                            LNG = x.LNG
                        }).ToList(),
                        SpecList = (from x in db.PipelineSpecValue
                                    join s in db.PipelineSpec
                                    on x.SpecUniqueID equals s.UniqueID
                                    where x.PipelineUniqueID == pipeline.UniqueID
                                    select new SpecModel
                                    {
                                        UniqueID = s.UniqueID,
                                        Description = s.Description,
                                        OptionUniqueID = x.SpecOptionUniqueID,
                                        Value = x.Value,
                                        Seq = x.Seq,
                                        OptionList = db.PipelineSpecOption.Where(o => o.SpecUniqueID == s.UniqueID).OrderBy(o => o.Seq).ToList()
                                    }).OrderBy(x => x.Description).ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var exists = db.Pipeline.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        db.Pipeline.Add(new Pipeline()
                        {
                            UniqueID = Model.UniqueID,
                            OrganizationUniqueID = Model.OrganizationUniqueID,
                            ID = Model.FormInput.ID,
                            Color = Model.FormInput.Color.Replace("#",""),
                            LastModifyTime = DateTime.Now
                        });

                        db.PipelineSpecValue.AddRange(Model.SpecList.Select(x => new PipelineSpecValue
                        {
                            PipelineUniqueID = Model.UniqueID,
                            SpecUniqueID = x.UniqueID,
                            SpecOptionUniqueID = x.OptionUniqueID,
                            Value = x.Value,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.Pipeline, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.PipelineID, Resources.Resource.Exists));
                    }
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var pipeline = db.Pipeline.First(x => x.UniqueID == UniqueID);

                    var organization = OrganizationDataAccessor.GetOrganization(pipeline.OrganizationUniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = pipeline.UniqueID,
                        OrganizationUniqueID = pipeline.OrganizationUniqueID,
                        ParentOrganizationFullDescription = organization.FullDescription,
                        SpecList = (from x in db.PipelineSpecValue
                                    join s in db.PipelineSpec
                                    on x.SpecUniqueID equals s.UniqueID
                                    where x.PipelineUniqueID == pipeline.UniqueID
                                    select new SpecModel
                                    {
                                        UniqueID = s.UniqueID,
                                        Description = s.Description,
                                        OptionUniqueID = x.SpecOptionUniqueID,
                                        Value = x.Value,
                                        Seq = x.Seq,
                                        OptionList = db.PipelineSpecOption.Where(o => o.SpecUniqueID == s.UniqueID).OrderBy(o => o.Seq).ToList()
                                    }).OrderBy(x => x.Seq).ToList(),
                        FormInput = new FormInput()
                        {
                            ID = pipeline.ID,
                            Color = "#"+pipeline.Color
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var pipeline = db.Pipeline.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.Pipeline.FirstOrDefault(x => x.OrganizationUniqueID == pipeline.OrganizationUniqueID && x.UniqueID != pipeline.UniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region Equipment
                        pipeline.ID = Model.FormInput.ID;
                        pipeline.Color = Model.FormInput.Color.Replace("#","");
                        pipeline.LastModifyTime = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region PipelineSpecValue
                        #region Delete
                        db.PipelineSpecValue.RemoveRange(db.PipelineSpecValue.Where(x => x.PipelineUniqueID == pipeline.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.PipelineSpecValue.AddRange(Model.SpecList.Select(x => new PipelineSpecValue
                        {
                            PipelineUniqueID = pipeline.UniqueID,
                            SpecUniqueID = x.UniqueID,
                            SpecOptionUniqueID = x.OptionUniqueID,
                            Value = x.Value,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();
                        #endregion
                        #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif
                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.Pipeline, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.PipelineID, Resources.Resource.Exists));
                    }
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult SavePageState(List<SpecModel> SpecList, List<string> PageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                int seq = 1;

                foreach (string pageState in PageStateList)
                {
                    string[] temp = pageState.Split(Define.Seperators, StringSplitOptions.None);

                    string specUniqueID = temp[0];
                    string optionUniqueID = temp[1];
                    string value = temp[2];

                    var spec = SpecList.First(x => x.UniqueID == specUniqueID);

                    spec.OptionUniqueID = optionUniqueID;
                    spec.Value = value;
                    spec.Seq = seq;

                    seq++;
                }

                SpecList = SpecList.OrderBy(x => x.Seq).ToList();

                result.ReturnData(SpecList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddSpec(List<SpecModel> SpecList, List<string> SelectedList, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(Define.Seperators, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var type = temp[1];
                        var specUniqueID = temp[2];

                        if (!string.IsNullOrEmpty(specUniqueID))
                        {
                            if (!SpecList.Any(x => x.UniqueID == specUniqueID))
                            {
                                var spec = db.PipelineSpec.First(x => x.UniqueID == specUniqueID);

                                SpecList.Add(new SpecModel()
                                {
                                    UniqueID = spec.UniqueID,
                                    Description = spec.Description,
                                    Seq = SpecList.Count + 1,
                                    OptionList = db.PipelineSpecOption.Where(x => x.SpecUniqueID == spec.UniqueID).OrderBy(x => x.Seq).ToList()
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(type))
                            {
                                var specList = db.PipelineSpec.Where(x => x.OrganizationUniqueID == organizationUniqueID && x.Type == type).ToList();

                                foreach (var spec in specList)
                                {
                                    if (!SpecList.Any(x => x.UniqueID == spec.UniqueID))
                                    {
                                        SpecList.Add(new SpecModel()
                                        {
                                            UniqueID = spec.UniqueID,
                                            Description = spec.Description,
                                            Seq = SpecList.Count + 1,
                                            OptionList = db.PipelineSpecOption.Where(x => x.SpecUniqueID == spec.UniqueID).OrderBy(x => x.Seq).ToList()
                                        });
                                    }
                                }
                            }
                            else
                            {
                                var availableOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);

                                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                                foreach (var organization in downStreamOrganizationList)
                                {
                                    if (availableOrganizationList.Any(x => x == organization))
                                    {
                                        var specList = db.PipelineSpec.Where(x => x.OrganizationUniqueID == organization).ToList();

                                        foreach (var spec in specList)
                                        {
                                            if (!SpecList.Any(x => x.UniqueID == spec.UniqueID))
                                            {
                                                SpecList.Add(new SpecModel()
                                                {
                                                    UniqueID = spec.UniqueID,
                                                    Description = spec.Description,
                                                    Seq = SpecList.Count + 1,
                                                    OptionList = db.PipelineSpecOption.Where(x => x.SpecUniqueID == spec.UniqueID).OrderBy(x => x.Seq).ToList()
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                SpecList = SpecList.OrderBy(x => x.Seq).ToList();

                result.ReturnData(SpecList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(string OrganizationUniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PipelineUniqueID, string.Empty }
                };

                using (PDbEntities pdb = new PDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var pipelineList = pdb.Pipeline.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var pipeline in pipelineList)
                        {
                            var treeItem = new TreeItem() { Title = pipeline.ID };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Pipeline.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = pipeline.ID;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineUniqueID] = pipeline.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }

                    using (DbEntities db = new DbEntities())
                    {
                        var organizationList = db.Organization.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var treeItem = new TreeItem() { Title = organization.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            var haveDownStreamOrganization = db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                            var havePipeline = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && pdb.Pipeline.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                            if (haveDownStreamOrganization || havePipeline)
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }
    }
}
