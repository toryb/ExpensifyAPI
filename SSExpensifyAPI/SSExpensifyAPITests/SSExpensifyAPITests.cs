using Microsoft.VisualStudio.TestTools.UnitTesting;
using SSExpensifyAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSExpensifyAPI.Tests
{
    [TestClass()]
    public class SSExpensifyAPITests
    {
        public string template = @"<#if (addHeader == true)>
            ReportDate,<#t>
            ReportID,<#t>
            ReportTitle,<#t>
            ProductLine,<#t>
            EmployeeID,<#t>
            EmployeeEmail,<#t>
            ExpenseDate,<#t>
            Merchant,<#t>
            ExpenseAmount,<#t>
            SplitTotal,<#t>
            CategoryName,<#t>
            CategoryID,<#t>
            MCC,<#t>
            Customer-Project,<#t>
            Customer Account,<#t>
            Customer Name,<#t>
            Project ID,<#t>
            Project Name,<#t>
            Comment,<#t>
            Billable,<#t>
            Reimbursable,<#t>
            Card,<#t>
            ReceiptURL,<#t>
            Transaction ID<#lt>
        </#if>
        <#list reports as report>
            <#assign splitTotal = 0>
            <#list report.transactionList as expense>
                <#if expense.convertedAmount?has_content>
                    <#assign amount = expense.convertedAmount/100>
                <#elseif expense.modifiedAmount?has_content>
                    <#assign amount = expense.modifiedAmount/100>
                <#else>
                    <#assign amount = expense.amount/100>
                </#if>
                <#assign NVPSource = expense.nameValuePairs.source!"""">
                <#if (NVPSource == ""split"")>
                    <#assign splitTotal = splitTotal + amount>
                <#else>
                    <#assign splitTotal = ""Not Split"">
                </#if>
            </#list>
            <#list report.transactionList as expense>
                <#if expense.modifiedCreated?has_content>
                    <#assign created = expense.modifiedCreated?date(""yyyy-MM-dd"")>
                <#else>
                    <#assign created = expense.created?date(""yyyy-MM-dd"")>
                </#if>
                <#if expense.convertedAmount?has_content>
                    <#assign amount = expense.convertedAmount/100>
                <#elseif expense.modifiedAmount?has_content>
                    <#assign amount = expense.modifiedAmount/100>
                <#else>
                    <#assign amount = expense.amount/100>
                </#if>
                ${report.created?date(""yyyy-MM-dd"")?string(""M/d/yy"")},<#t>
                ${report.reportID},<#t>
                ${report.reportName},<#t>
                AX-10,<#t>
                ${report.submitterUserID},<#t>
                ${report.accountEmail},<#t>
                ${created?string(""M/d/yy"")},<#t>
                ${expense.merchant},<#t>
                ${amount?string(""0.00"")},<#t>
                ${splitTotal},<#t>
                ${expense.category},<#t>
                ${expense.categoryGlCode},<#t>
                ${expense.mcc},<#t>
                ${expense.ntag1} | ${expense.ntag2},<#t>
                ${expense.ntag1GlCode},<#t>
                ${expense.ntag1},<#t>
                ${expense.ntag2GlCode},<#t>
                ${expense.ntag2},<#t>
                ${expense.comment},<#t>
                ${expense.billable?string(""TRUE"", ""FALSE"")},<#t>
                ${expense.reimbursable?string(""TRUE"", ""FALSE"")},<#t>
                ${expense.cardName},<#t>
                ${expense.receiptObject.url!""""},<#t>
                ${expense.transactionID}<#lt>
            </#list>
        </#list>";

        [TestMethod()]
        public void MakeReportExportAndDownloadRequestTest()
        {

            SSExpensifyAPI api = new SSExpensifyAPI();

            ExpensifyExportReportJobRequest request = new ExpensifyExportReportJobRequest();
            request.credentials.partnerUserID = "aa_expenses_stoneridgesoftware_com";
            request.credentials.partnerUserSecret = "7d87861227ff7ecc00f52ac0c96967807303b205";
            request.test = "true";
            request.inputSettings.reportState = "APPROVED,REIMBURSED";
            request.inputSettings.filters.startDate = "2015-12-01";
            request.inputSettings.filters.markedAsExported = "Stoneridge Software";
            request.outputSettings.fileExtension = "csv";
            request.outputSettings.fileBasename = "ExpenseExport";
            request.onFinish = new Onfinish[1] { new MarkAsExportFinishAction() { label = "Stoneridge Software" } };

            string fileName = api.MakeReportExportRequest(request, api.ExportTemplate);
            Assert.AreEqual(200, api.Status.Status, api.Status.StatusDescription);
            Assert.IsNotNull(fileName);
            Assert.IsFalse(string.IsNullOrEmpty(fileName));

            ExpensifyDownloadJobRequest downloadRequest = new ExpensifyDownloadJobRequest();
            downloadRequest.credentials.partnerUserID = "aa_expenses_stoneridgesoftware_com";
            downloadRequest.credentials.partnerUserSecret = "7d87861227ff7ecc00f52ac0c96967807303b205";
            downloadRequest.fileName = fileName;
            api.MakeDownloadRequest(downloadRequest);
            Assert.AreEqual(200, api.Status.Status, api.Status.StatusDescription);
            Assert.IsTrue(System.IO.File.Exists(fileName));
            System.IO.File.Delete(fileName);
        }

        [TestMethod()]
        public void MakeCreatePolicyRequestTest()
        {
            SSExpensifyAPI api = new SSExpensifyAPI();
            ExpensifyCreatePolicyJobRequest jobRequest = new ExpensifyCreatePolicyJobRequest();
            jobRequest.credentials.partnerUserID = "aa_expenses_stoneridgesoftware_com";
            jobRequest.credentials.partnerUserSecret = "7d87861227ff7ecc00f52ac0c96967807303b205";

            jobRequest.test = "true";
            jobRequest.inputSettings.policyName = "MakeCreatePolicyRequestTest";

            string policyID = api.MakeCreatePolicyRequest(jobRequest);
            Assert.AreEqual(200, api.Status.Status);
            Assert.IsNotNull(policyID);
            Assert.IsFalse(string.IsNullOrEmpty(policyID));
        }

        [TestMethod()]
        public void InvalidMakeCreatePolicyRequestTest()
        {
            SSExpensifyAPI api = new SSExpensifyAPI();
            ExpensifyCreatePolicyJobRequest jobRequest = new ExpensifyCreatePolicyJobRequest();
            jobRequest.credentials.partnerUserID = "aa_expenses_stoneridgesoftware_com";
            jobRequest.credentials.partnerUserSecret = "7d87861227ff7ecc00f52ac0c96967807303b205";
            jobRequest.test = "true";

            string policyID = api.MakeCreatePolicyRequest(jobRequest);
            Assert.AreEqual(410, api.Status.Status, api.Status.StatusDescription);
            Assert.IsNotNull(policyID);
            Assert.IsTrue(string.IsNullOrEmpty(policyID));
        }

        [TestMethod()]
        public void MakeUpdatePolicyRequestCategoriesTest()
        {
            SSExpensifyAPI api = new SSExpensifyAPI();
            // api.AuthID = "aa_toryb_runestone_net";
            // api.AuthToken = "c61256ea6c865017fb5e358ff42e78b103b27d14";

            // First Create the Policy
            ExpensifyCreatePolicyJobRequest jobRequest = new ExpensifyCreatePolicyJobRequest();
            jobRequest.credentials.partnerUserID = "aa_expenses_stoneridgesoftware_com";
            jobRequest.credentials.partnerUserSecret = "7d87861227ff7ecc00f52ac0c96967807303b205";

            jobRequest.test = "true";
            jobRequest.inputSettings.policyName = "MakeUpdatePolicyRequestTest";

            string policyID = api.MakeCreatePolicyRequest(jobRequest);
            Assert.AreEqual(200, api.Status.Status, api.Status.StatusDescription);
            Assert.IsNotNull(policyID);
            Assert.IsFalse(string.IsNullOrEmpty(policyID));

            // Now modify the policy
            ExpensifyUpdatePolicyJobRequest updateJobRequest = new ExpensifyUpdatePolicyJobRequest();
            updateJobRequest.credentials.partnerUserID = "aa_expenses_stoneridgesoftware_com";
            updateJobRequest.credentials.partnerUserSecret = "7d87861227ff7ecc00f52ac0c96967807303b205";

            updateJobRequest.inputSettings.policyID = policyID;
            Categories categories = new Categories();
            categories.action = "replace";
            categories.data = new CategoriesData[]
            {
                new CategoriesData(){name="Transportation", enabled = false},
                new CategoriesData(){name="Lodging", enabled = false, areCommentsRequired=true, commentHint="Enter a name and number of nights",maxExpenseAmount=40000},
                new CategoriesData(){name="Mileage", enabled = false},
                new CategoriesData(){name="Meals", enabled = false},
                new CategoriesData(){name="Other", enabled = false, areCommentsRequired=true, commentHint="Enter a name a comment"}
            };
            updateJobRequest.categories = categories;
            api.MakeUpdatePolicyRequest(updateJobRequest);
            Assert.AreEqual(200, api.Status.Status, api.Status.StatusDescription);
        }

        [TestMethod()]
        public void MakeReportExportRequestTest()
        {
            SSExpensifyAPI api = new SSExpensifyAPI();

            ExpensifyExportReportJobRequest request = new ExpensifyExportReportJobRequest();
            request.credentials.partnerUserID = "aa_expenses_stoneridgesoftware_com";
            request.credentials.partnerUserSecret = "7d87861227ff7ecc00f52ac0c96967807303b205";
            request.test = "true";
            request.inputSettings.reportState = "APPROVED,REIMBURSED";
            request.inputSettings.filters.startDate = "2015-12-01";
            request.inputSettings.filters.markedAsExported = "Stoneridge Software";
            request.outputSettings.fileExtension = "csv";
            request.outputSettings.fileBasename = "ExpenseExport";
            request.onFinish = new Onfinish[1] { new MarkAsExportFinishAction() { label = "Stoneridge Software" } };

            string fileName = api.MakeReportExportRequest(request, api.ExportTemplate);
            Assert.AreEqual(200, api.Status.Status, api.Status.StatusDescription);
            Assert.IsNotNull(fileName);
            Assert.IsFalse(string.IsNullOrEmpty(fileName));
        }

        /// <summary>
        /// This test will attempt to test the basic path for updating an expense report in the 
        /// Approved status to reimbursed status.
        /// </summary>
        [TestMethod()]
        public void MakeUpdateReportStatusRequestTest()
        {
            // need to be able to find or crete a report in an Approved status
            // Get the report ID
            // update the status
            // get the report again
            // Verify the status
            Assert.Fail();
        }

        /// <summary>
        /// This test will get the list of policies in Expensify
        /// </summary>
        [TestMethod()]
        public void MakeGetPolicyListRequestTest()
        {
            // Build the job request
            SSExpensifyAPI api = new SSExpensifyAPI();
            ExpensifyGetPolicyListJobRequest request = new ExpensifyGetPolicyListJobRequest()
            {
                credentials = new Credentials()
                {
                    partnerUserID = "aa_expenses_stoneridgesoftware_com",
                    partnerUserSecret = "7d87861227ff7ecc00f52ac0c96967807303b205"
                },
                test = "true"
            };
            // Obtain the list of policies
            PolicyListInfo[] list = api.MakeGetPolicyListRequest(request);

            Assert.IsNotNull(list, "Policy List is NULL");
            bool policyFound = false;
            // Verify that the stoneridge policy exists
            foreach (PolicyListInfo listInfo in list)
            {
                if (listInfo.name == "Stoneridge Software")
                {
                    policyFound = true;
                    break;
                }
            }
            Assert.IsTrue(policyFound, "Stoneridge Software policy not found.");
            policyFound = false;
            // Verify that the stoneridge policy exists
            foreach (PolicyListInfo listInfo in list)
            {
                if (listInfo.name == "Stoneridge Software Test")
                {
                    policyFound = true;
                    break;
                }
            }
            Assert.IsTrue(policyFound, "Stoneridge Software Test policy not found.");
        }
    }
}