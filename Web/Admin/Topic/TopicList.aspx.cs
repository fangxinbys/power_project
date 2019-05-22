﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Web;
using FineUIPro;
using Maticsoft.DBUtility;
using System.Data.SqlClient;
using System.Text;

namespace Maticsoft.Web.Admin.Topic
{
    public partial class TopicList : PageBase
    {
        string tId = "";
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(Request.QueryString["Id"]))
            {
                tId = Request.QueryString["Id"]; 
            }
            if (!IsPostBack)
            {
                BindTree();

                LoadData();
                
            }
        }

        protected void GridDpt_PageIndexChange(object sender, FineUIPro.GridPageEventArgs e)
        {
            LoadData();
        }
        protected void BindTree()
        {
            TreeDpt.Nodes.Clear();
            int dptId= GetIdentityUser().dptId;
            IDataParameter[] parameters =new IDataParameter[] { new SqlParameter("@dptId", dptId) }; 
            DataSet ds = DbHelperSQL.RunProcedure("GetChildrenDptTree", parameters, "dptTree");
            ds.Relations.Add("TreeRelation", ds.Tables[0].Columns["dptId"], ds.Tables[0].Columns["dptFatherId"],false);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                if (row["dptId"].ToString()==dptId.ToString())
                {
                    FineUIPro.TreeNode node = new FineUIPro.TreeNode();
                    node.NodeID = row["dptId"].ToString();
                    node.Text = row["dptName"].ToString();
                    node.EnableClickEvent = true;
                    TreeDpt.Nodes.Add(node);
                    ResolveSubTree(row, node);
                }
            }
            TreeDpt.SelectedNodeID = dptId.ToString();
        }
        private void ResolveSubTree(DataRow dataRow, FineUIPro.TreeNode treeNode)
        {
            DataRow[] rows = dataRow.GetChildRows("TreeRelation");
            if (rows.Length > 0)
            {
                // 如果是目录，则默认展开
                treeNode.Expanded = true;
                foreach (DataRow row in rows)
                {
                    FineUIPro.TreeNode node = new FineUIPro.TreeNode();
                    node.NodeID = row["dptId"].ToString();
                    node.Text = row["dptName"].ToString();
                    node.EnableClickEvent = true;
                    treeNode.Nodes.Add(node); 
                    ResolveSubTree(row, node);
                }
            }
        }

        protected void LoadData()
        {


            Maticsoft.BLL.tTopic BLLGET = new Maticsoft.BLL.tTopic();
            string sortField = GridDpt.SortField;
            string sortDirection = GridDpt.SortDirection;
            StringBuilder sb = new StringBuilder();
            string dptlist = GetTreeNode(TreeDpt.SelectedNode, sb, true).ToString();
            GridDpt.RecordCount = BLLGET.GetRecordCount(string.Format(" policyDptId in ({0}) and policyType="+tId, dptlist.Substring(0, dptlist.Length - 1)));

            DataView view = BLLGET.GetListByPage(string.Format(" policyDptId in ({0}) and policyType=" + tId, dptlist.Substring(0, dptlist.Length - 1)), " Id desc ", GridDpt.PageIndex * GridDpt.PageSize, (GridDpt.PageIndex + 1) * GridDpt.PageSize).Tables[0].DefaultView;
            view.Sort = String.Format("{0} {1}", sortField, sortDirection);
            GridDpt.DataSource = view.ToTable();
            GridDpt.DataBind();
        }
      
         


        protected void TreeDpt_NodeCommand(object sender, TreeCommandEventArgs e)
        {
            LoadData();
        }

        protected void GridDpt_RowCommand(object sender, GridCommandEventArgs e)
        {
           
            int deptID  = GetSelectedDataKeyID(GridDpt);


            if (e.CommandName == "Delete")
            {


                 
                BLL.tTopic uBLL = new BLL.tTopic();
                if (uBLL.GetModel(deptID).isCheck=="已审核")
                {
                    Alert.ShowInTop("已经审核,无法删除！");
                    return;
                } 
                bool isTrue = uBLL.Delete(deptID); 
                if (!isTrue)
                {
                    Alert.ShowInTop("删除失败！");
                    return;
                }
                else
                {
                    LoadData();
                }
            }
            if (e.CommandName == "Edit")
            {
                Window1.Title = "决策管理";
                string openUrl = String.Format("./TopicEdit.aspx?Id={0}&tId="+tId, HttpUtility.UrlEncode(deptID.ToString()));
                PageContext.RegisterStartupScript(Window1.GetSaveStateReference(deptID.ToString()) + Window1.GetShowReference(openUrl));
            }


        }

        protected void GridDpt_Sort(object sender, GridSortEventArgs e)
        {
            LoadData();
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            Window1.Title = "决策管理";
            string openUrl = String.Format("./TopicEdit.aspx?dptId={0}&tId=" + tId, HttpUtility.UrlEncode(TreeDpt.SelectedNodeID));
            PageContext.RegisterStartupScript(Window1.GetSaveStateReference(TreeDpt.SelectedNodeID) + Window1.GetShowReference(openUrl));
      
        }

        protected void Window1_Close(object sender, WindowCloseEventArgs e)
        {
            Alert.ShowInTop("保存成功");
            BindTree();
            LoadData();
        }
    }
}