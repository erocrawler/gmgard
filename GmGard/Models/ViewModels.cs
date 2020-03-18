using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace GmGard.Models
{
    public class HeaderDisplay
    {
        public Category category { get; set; }
        public int newItems { get; set; }
    }

    public class UsersRating
    {
        public int BlogID { get; set; }
        public string credential { get; set; }
        public bool HasPost { get; set; }
        public bool RateWithAccount { get; set; }
        public BlogRating Rating { get; set; }
    }

    public class SortOptions
    {
        public SortOptions(string sort = "", bool hasScore = false)
        {
            if (sort == null)
            {
                sort = string.Empty;
            }
            HasScore = hasScore;
            CurrentSort = sort.Split(':')[0];
        }

        public string DateSortParm => (string.IsNullOrEmpty(CurrentSort) || CurrentSort == "Date_desc") ? "Date" : "Date_desc";
        public string AddDateSortParm => CurrentSort == "AddDate_desc" ? "AddDate" : "AddDate_desc";
        public string VisitSortParm => CurrentSort == "Visit_desc" ? "Visit" : "Visit_desc";
        public string PostSortParm => CurrentSort == "Post_desc" ? "Post" : "Post_desc";
        public string RateSortParm => CurrentSort == "Rate_desc" ? "Rate" : "Rate_desc";
        public string UpdateSortParm => CurrentSort == "Update_desc" ? "Update" : "Update_desc";
        public string ScoreSortParm => (string.IsNullOrEmpty(CurrentSort) || CurrentSort == "Score_desc") ? "Score" : "Score_desc";

        public string CurrentSort
        {
            get { return sort; }
            set
            {
                sort = value;
                GetSortName();
            }
        }

        public string NextSort { get; private set; }
        public string SortName { get; private set; }
        public bool IsDescending { get; private set; }
        public bool HasScore { get; }

        private string sort;

        private void GetSortName()
        {
            string name, nextsort;
            bool isDesc;
            switch (sort)
            {
                case "Date":
                    name = "发布日期";
                    nextsort = "Date_desc";
                    isDesc = false;
                    break;

                case "Date_desc":
                    name = "发布日期";
                    nextsort = "Date";
                    isDesc = true;
                    break;

                case "Update_desc":
                    name = "更新日期";
                    nextsort = "Update";
                    isDesc = true;
                    break;

                case "Update":
                    name = "更新日期";
                    nextsort = "Update_desc";
                    isDesc = false;
                    break;

                case "Visit_desc":
                    name = "浏览数";
                    nextsort = "Visit";
                    isDesc = true;
                    break;

                case "Visit":
                    name = "浏览数";
                    nextsort = "Visit_desc";
                    isDesc = false;
                    break;

                case "Post":
                    name = "评论数";
                    nextsort = "Post_desc";
                    isDesc = false;
                    break;

                case "Post_desc":
                    name = "评论数";
                    nextsort = "Post";
                    isDesc = true;
                    break;

                case "Rate":
                    name = "评分";
                    nextsort = "Rate_desc";
                    isDesc = false;
                    break;

                case "Rate_desc":
                    name = "评分";
                    nextsort = "Rate";
                    isDesc = true;
                    break;

                case "AddDate":
                    name = "添加日期";
                    nextsort = "AddDate_desc";
                    isDesc = false;
                    break;

                case "AddDate_desc":
                    name = "添加日期";
                    nextsort = "AddDate";
                    isDesc = true;
                    break;

                case "Score":
                    name = "相关度";
                    nextsort = "Score_desc";
                    isDesc = false;
                    break;

                case "Score_desc":
                    name = "相关度";
                    nextsort = "Score";
                    isDesc = true;
                    break;

                default:
                    name = HasScore ? "相关度" : "发布日期";
                    nextsort = HasScore ? "Score" : "Date";
                    isDesc = true;
                    break;
            }
            NextSort = nextsort;
            SortName = name;
            IsDescending = isDesc;
        }
    }

    public class ErrorContextModel
    {
        public Exception Exception;
        public string ControllerActionPath;
    }
}