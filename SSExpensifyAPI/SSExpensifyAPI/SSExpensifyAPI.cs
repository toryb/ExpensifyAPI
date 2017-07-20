using System;
using System.Net;
using System.Collections.Specialized;
using System.Reflection;
using Newtonsoft.Json;

namespace SSExpensifyAPI
{
    public class ErrorResponse
    {
        public string responseMessage { get; set; }
        public int responseCode { get; set; }
    }

    public class StatusResponse
    {
        public int Status { get; set; }
        public string StatusDescription { get; set; }
    }

    public class SSExpensifyAPI
    {
        private ExpensifyJobRequest jobRequest_;

        #region Properties
        public string ExportTemplate { get; set; }
        public string XMLExportTemplate { get; set; }
        public string JSONExportTemplate { get; set; }
        public string UrlBase { get; set; }
        public string AuthID { get; set; }
        public string AuthToken { get; set; }
        public NameValueCollection FormParameters { get; set; }
        public NameValueCollection QueryParameters { get; set; }
        /// <summary>
        /// The response headers in a string format
        /// </summary>
        public NameValueCollection ResponseHeaders { get; set; }
        /// <summary>
        /// Returns the status code and description
        /// </summary>
        public StatusResponse Status { get; set; }
        public string RawResponse { get; set; }
        public ExpensifyJobRequest JobRequest
        {
            get
            {
                return jobRequest_;
            }
            set
            {
                jobRequest_ = value;
                // See if we have Auth values to update into the Job Request
                if (!string.IsNullOrEmpty(AuthID) && !string.IsNullOrEmpty(AuthToken))
                {
                    jobRequest_.credentials.partnerUserID = AuthID;
                    jobRequest_.credentials.partnerUserSecret = AuthToken;
                }
            }
        }
        #endregion Properties

        #region Constructors
        public SSExpensifyAPI()
        {
            UrlBase = @"https://integrations.expensify.com/Integration-Server/ExpensifyIntegrations";
            ExportTemplate = @"<#if (addHeader == true)>
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
                <#if expense.modifiedMerchant?has_content>
                    <#assign merchant = expense.modifiedMerchant>
                <#else>
                    <#assign merchant = expense.merchant>
                </#if>
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
                ""${report.submitted?date(""yyyy-MM-dd"")?string(""M/d/yy"")}"",<#t>
                ""${report.reportID}"",<#t>
                ""${report.reportName?replace(""\"""", """")}"",<#t>
                ""AX-10"",<#t>
                ""${report.submitterUserID}"",<#t>
                ""${report.accountEmail}"",<#t>
                ""${created?string(""M/d/yy"")}"",<#t>
                ""${merchant?replace(""\"""", """")}"",<#t>
                ""${amount?string(""0.00"")}"",<#t>
                ""${splitTotal}"",<#t>
                ""${expense.category}"",<#t>
                ""${expense.categoryGlCode}"",<#t>
                ""${expense.mcc}"",<#t>
                ""${expense.ntag1?replace(""\"""", """")} | ${expense.ntag2?replace(""\"""", """")}"",<#t>
                ""${expense.ntag1GlCode}"",<#t>
                ""${expense.ntag1?replace(""\"""", """")}"",<#t>
                ""${expense.ntag2GlCode}"",<#t>
                ""${expense.ntag2?replace(""\"""", """")}"",<#t>
                ""${expense.comment?replace(""\"""", """")}"",<#t>
                ""${expense.billable?string(""TRUE"", ""FALSE"")}"",<#t>
                ""${expense.reimbursable?string(""TRUE"", ""FALSE"")}"",<#t>
                ""${expense.cardName}"",<#t>
                ""${expense.receiptObject.url!""""}"",<#t>
                ""${expense.transactionID}""<#lt>
            </#list>
        </#list>";

            XMLExportTemplate =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Reports>
<#list reports as report>
<Report id =${report.reportID}>
<ReportSubmittedDate>${report.submitted? date(""yyyy-MM-dd"") ? string(""M/d/yy"")}</ReportSubmittedDate>
<ReportID>${report.reportID}</ReportID>
<ReportTitle>${report.reportName}</ReportTitle>
<EmployeeID>${report.submitterUserID}</EmployeeID>
<EmployeeEmail>${report.accountEmail}</EmployeeEmail>
<Expenses>
<#list report.transactionList as expense>
<#if expense.modifiedMerchant?has_content>
    <#assign merchant = expense.modifiedMerchant>
<#else>
    <#assign merchant = expense.merchant>
</#if>
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
<Expense id=${expense.transactionID}>
  <ExpenseDate>${created?string(""M/d/yy"")}</ExpenseDate>
  <Merchant>${merchant}</Merchant>
  <ExpenseAmount>${amount?string(""0.00"")}</ExpenseAmount>
  <CategoryName>${expense.category}</CategoryName>
  <CategoryID>${expense.categoryGlCode}</CategoryID>
  <MCC>${expense.mcc}</MCC>
  <CustomerAccount>${expense.ntag1GlCode}</CustomerAccount>
  <CustomerName>${expense.ntag1}</CustomerName>
  <ProjectID>${expense.ntag2GlCode}</ProjectID>
  <ProjectName>${expense.ntag2}</ProjectName>
  <Comment>${expense.comment}</Comment>
  <Billable>${expense.billable?string(""TRUE"", ""FALSE"")}</Billable>
  <Reimbursable>${expense.reimbursable?string(""TRUE"", ""FALSE"")}</Reimbursable>
  <Card>${expense.cardName}</Card>
  <ReceiptURL>${expense.receiptObject.url!""""}</ReceiptURL>
  <TransactionID>${expense.transactionID}</TransactionID>
</expense>
</#list>
</expenses>
</report>
</#list>
</reports>";

        JSONExportTemplate = 
@"{
  ""Reports"" : [
    {
		<#assign firstReport=""true"">
		<#list reports as report>
		<#if firstReport == ""false"">},
    { </#if>
      ""ReportDate"" : ""${report.created?date(""yyyy-MM-dd"")?string(""M/d/yy"")}"",
			""ReportID"" : ""${report.reportID}"",
			""ReportTitle"" : ""${report.reportName}"",
			""EmployeeID"" : ""${report.submitterUserID}"",
			""EmployeeEmail"" : ""${report.accountEmail}"",
			""Expenses"" : [
        {
			<#assign firstExpense=""true"">
			<#list report.transactionList as expense>
        <#if expense.modifiedMerchant?has_content>
            <#assign merchant = expense.modifiedMerchant>
        <#else>
            <#assign merchant = expense.merchant>
        </#if>
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
				<#if firstExpense == ""false"">},
        {</#if>
				  ExpenseDate : ""${ created?string(""M/d/yy"")}"",
				  Merchant : ""${merchant}"",
				  ExpenseAmount : ""${amount?string(""0.00"")}"",
				  CategoryName : ""${expense.category}"",
				  CategoryID : ""${expense.categoryGlCode}"",
				  MCC : ""${expense.mcc}"",
				  Customer-Project : ""${expense.ntag1} | ${expense.ntag2}"",
				  CustomerAccount : ""${expense.ntag1GlCode}"",
				  CustomerName : ""${expense.ntag1}"",
				  ProjectID : ""${expense.ntag2GlCode}"",
				  ProjectName : ""${expense.ntag2}"",
				  Comment : ""${expense.comment}"",
				  Billable : ""${expense.billable?string(""TRUE"", ""FALSE"")}"",
				  Reimbursable : ""${expense.reimbursable?string(""TRUE"", ""FALSE"")}"",
				  Card : ""${expense.cardName}"",
				  ReceiptURL : ""${expense.receiptObject.url!""""}"",
				  TransactionID : ""${expense.transactionID}""
				<#assign firstExpense=""false"">
			</#list>
			  }
      ]
			<#assign firstReport=""false"">
		</#list>
	  }
  ]
}";



        }
        #endregion Constructors

        #region Public Methods
        /// <summary>
        /// The method is the base method for making all Job Requests.
        /// </summary>
        /// <returns>The raw UTF8 encoded response from the web post</returns>
        /// <remarks>This method adds the necessary headers and uses the <see cref="QueryParameters"/>
        /// and <see cref="FormParameters"/> if they have values but does not add values
        /// to those properties. It will also populate <see cref="ResponseHeaders"/> and 
        /// <see cref="RawResponse"/> from the response returned from the Post.</remarks>
        public byte[] MakeJobRequest()
        {
            // Make sure this class is ready to make a request
            if (string.IsNullOrEmpty(UrlBase)) throw new ApplicationException("SSExpensifyAPI.UrlBase cannot be blank.");
            if (null == JobRequest) throw new ApplicationException("SSExpensifyAPI.JobRequest cannot be null.");
            using (var w = new WebClient())
            {
                System.Net.ServicePointManager.Expect100Continue = false;
                w.BaseAddress = UrlBase;
                w.Headers.Add("Host", "integrations.expensify.com");
                w.Headers.Add("Cache-Control", "max-age=0");
                w.Headers.Add("Accept", @"application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                w.Headers.Add("Accept-Encoding", "gzip, deflate");
                w.Headers.Add("Accept-Language", "en-US,en;q=0.8,fr;q=0.6");

                if (null != QueryParameters) w.QueryString = QueryParameters;
                byte[] response = w.UploadValues(string.Empty,
                    null == FormParameters ? new System.Collections.Specialized.NameValueCollection() : FormParameters);

                this.ResponseHeaders = w.ResponseHeaders as NameValueCollection;
                string statusDescription;
                int status = GetStatusCode(w, out statusDescription);
                this.Status = new StatusResponse() { Status = status, StatusDescription = statusDescription };

                // unfortunately Expensify returns a status of "Ok" or 200, but the response may contain a response code of 500
                RawResponse = System.Text.Encoding.UTF8.GetString(response);

                if (RawResponse.StartsWith("{\"responseMessage\""))
                {
                    ErrorResponse r = JsonConvert.DeserializeObject<ErrorResponse>(RawResponse);
                    Status.Status = r.responseCode;
                    Status.StatusDescription = r.responseMessage;
                }

                return response;
            }
        }

        /// <summary>
        /// Verifies all required parameters and values for a Report Export job request
        /// and then calls <see cref="MakeJobRequest"/>.
        /// </summary>
        /// <returns>The raw UTF8 encoded response from the web post. 
        /// This should be the file name for the output of the export.</returns>
        /// <remarks>This method assumes the caller has assigned the <see cref="JobRequest"/>
        /// and set the <see cref="FormParameters"/></remarks>
        public byte[] MakeReportExportRequest()
        {
            if (string.IsNullOrEmpty(UrlBase)) throw new ApplicationException("SSExpensifyAPI.UrlBase cannot be blank.");
            if (null == JobRequest) throw new ApplicationException("SSExpensifyAPI.JobRequest cannot be null.");
            if (!(JobRequest is ExpensifyExportReportJobRequest)) throw new ApplicationException("SSExpensifyAPI.JobRequest must be ExpensifyReportExportJobRequest for the MakeReportExportRequest method.");
            if (null == FormParameters) throw new ApplicationException("SSExpensifyAPI.FormParameters cannot be null.");
            if (FormParameters.Count < 0) throw new ApplicationException("SSExpensifyAPI.FormParameters must contain a requestJobDescription and a template.");

            return MakeJobRequest();
        }

        /// <summary>
        /// This method uses <paramref name="jobRequest"/> and <paramref name="template"/>
        /// to update the <see cref="FormParameters"/> and call <seealso cref="MakeReportExportRequest"/>
        /// </summary>
        /// <param name="jobRequest"></param>
        /// <param name="template"></param>
        /// <returns>The file name for the output of the export as a 
        /// response from the web post as an decoded string</returns>
        public string MakeReportExportRequest(ExpensifyExportReportJobRequest jobRequest, 
            string template)
        {
            JobRequest = jobRequest;
            InitializeDefaultCallValues();
            UpdateFormParameters(jobRequest);
            FormParameters.Add("template", template);
            string result = System.Text.Encoding.UTF8.GetString(MakeReportExportRequest());
            return result;
        }

        /// <summary>
        /// This method uses the <paramref name="jobRequest"/> and <paramref name="destination"/>
        /// to set the <see cref="FormParameters"/> and call <seealso cref="MakeJobRequest"/> to
        /// obtain a file that was created as an output to a previous job request. 
        /// See also <seealso cref="MakeReportExportRequest"/>
        /// </summary>
        /// <param name="jobRequest"></param>
        /// <param name="destination"></param>
        public void MakeDownloadRequest(ExpensifyDownloadJobRequest jobRequest, string destination = "")
        {
            JobRequest = jobRequest;
            InitializeDefaultCallValues();
            UpdateFormParameters(jobRequest);

            byte[] response = MakeJobRequest();
            using (System.IO.StreamWriter outputFile = System.IO.File.CreateText(string.IsNullOrEmpty(destination) ? jobRequest.fileName : destination))
            {
                outputFile.Write(System.Text.Encoding.UTF8.GetString(response));
                outputFile.Close();
            }
        }

        /// <summary>
        /// This method will make a job request to create a new policy.
        /// </summary>
        /// <param name="jobRequest"></param>
        /// <returns>The policy ID if succesfull, otherwise an empty string.</returns>
        public string MakeCreatePolicyRequest(ExpensifyCreatePolicyJobRequest jobRequest)
        {
            JobRequest = jobRequest;
            InitializeDefaultCallValues();
            UpdateFormParameters(jobRequest);


            byte[] response = MakeJobRequest();

            if (200 == Status.Status)
            {
                CreatePolicyResponse r = JsonConvert.DeserializeObject<CreatePolicyResponse>(RawResponse);
                string PolicyID = r.policyID;
                System.Diagnostics.Debug.Assert(jobRequest.inputSettings.policyName == r.policyName);
                return PolicyID;
            }
            return string.Empty;
        }

        /// <summary>
        /// This method will make a job request to update a policy.
        /// </summary>
        /// <param name="jobRequest"></param>
        public void MakeUpdatePolicyRequest(ExpensifyUpdatePolicyJobRequest jobRequest)
        {
            JobRequest = jobRequest;
            InitializeDefaultCallValues();
            UpdateFormParameters(jobRequest);

            MakeJobRequest();

        }

        /// <summary>
        /// This method will make a Policy List job request.
        /// </summary>
        /// <param name="jobRequest"></param>
        /// <returns>An array of <see cref="PolicyInfo"/> if succesfull, otherwise null.</returns>
        public PolicyListInfo[] MakeGetPolicyListRequest(ExpensifyGetPolicyListJobRequest jobRequest)
        {
            JobRequest = jobRequest;
            InitializeDefaultCallValues();
            UpdateFormParameters(jobRequest);

            byte[] response = MakeJobRequest();

            if(200 == Status.Status)
            {
                GetPolicyListResponse r = JsonConvert.DeserializeObject<GetPolicyListResponse>(RawResponse);
                return r.policyList;
            }
            return null;
        }

        /// <summary>
        /// This method will make a job request to update an expense report status.
        /// </summary>
        /// <param name="jobRequest"></param>
        /// <returns></returns>
        public UpdateReportStatusResponse MakeUpdateReportStatusRequest(ExpensifyUpdateReportStatusJobRequest jobRequest)
        {
            if ("update" != jobRequest.type) throw new ApplicationException(
                string.Format("SSExpensifyAPI.MakeUpdateReportStatusRequest: Invalid parameter value\n\tParamter: jobRequest.type\n\tValid Values:\"update\"\n\tActual Value: {0}.", 
                jobRequest.type));
            if ("reportStatus" != jobRequest.inputSettings.type) throw new ApplicationException(
                string.Format("SSExpensifyAPI.MakeUpdateReportStatusRequest: Invalid parameter value\n\tParamter: jobRequest.inputSettings.type\n\tValid Values:\"reportStatus\"\n\tActual Value: {0}.",
                jobRequest.inputSettings.type));
            if ("REIMBURSED" != jobRequest.inputSettings.status) throw new ApplicationException(
                string.Format("SSExpensifyAPI.MakeUpdateReportStatusRequest: Invalid parameter value\n\tParamter: jobRequest.inputSettings.status\n\tValid Values:\"REIMBURSED\"\n\tActual Value: {0}.",
                jobRequest.inputSettings.status));

            JobRequest = jobRequest;
            InitializeDefaultCallValues();
            UpdateFormParameters(jobRequest);

            byte[] response = MakeJobRequest();

            if (200 == Status.Status || 207 == Status.Status)
            {
                UpdateReportStatusResponse r = JsonConvert.DeserializeObject<UpdateReportStatusResponse>(RawResponse);
                return r;
            }
            return null;
        }
        #endregion Public Methods

        #region Private Helper Methods
        /// <summary>
        /// Adds the "requestJobDescription" Form Parameter for the Post method
        /// </summary>
        /// <param name="jobRequest">The ExpensifyJobRequest to serialize into the "requestJobDescription" Form Parameter</param>
        private void UpdateFormParameters(ExpensifyJobRequest jobRequest)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            FormParameters = new System.Collections.Specialized.NameValueCollection() 
                {
                    {"requestJobDescription", JsonConvert.SerializeObject(jobRequest, settings)}
                };
        }

        /// <summary>
        /// Clears any residual values from previous calls
        /// </summary>
        /// <remarks>
        /// Clears the following values:
        ///     FormParameters
        ///     QueryParameters
        ///     ResponseHeaders
        ///     Status
        ///     RawResponse
        /// </remarks>
        private void InitializeDefaultCallValues()
        {
            if (null != FormParameters) FormParameters.Clear();
            if (null != QueryParameters) QueryParameters.Clear();
            if (null != ResponseHeaders) ResponseHeaders.Clear();
            if (null != Status)
            {
                Status.Status = 0;
                Status.StatusDescription = string.Empty;
            }
            if (null != RawResponse) RawResponse = string.Empty;
        }

        /// <summary>
        /// Uses reflection to obtain the status code and description from the webresponse of 
        /// <paramref name="client"/>
        /// </summary>
        /// <param name="client">The WebClient from which to obtain the response</param>
        /// <param name="statusDescription">The description provided in the HTTPWebResponse</param>
        /// <returns>The StatusCode of the HttpWebResponse or 0(zero) if the response does not exist</returns>
        private static int GetStatusCode(WebClient client, out string statusDescription)
        {
            FieldInfo responseField = client.GetType().GetField("m_WebResponse", BindingFlags.Instance | BindingFlags.NonPublic);

            if (responseField != null)
            {
                HttpWebResponse response = responseField.GetValue(client) as HttpWebResponse;

                if (response != null)
                {
                    statusDescription = response.StatusDescription;
                    return (int)response.StatusCode;
                }
            }

            statusDescription = null;
            return 0;
        }
        #endregion Private Helper Methods

    }

    /// <summary>
    /// This class is the base class for all specific job request types.
    /// </summary>
    /// <remarks>
    /// See https://integrations.expensify.com/Integration-Server/doc/#request-format
    /// </remarks>
    public class ExpensifyJobRequest
    {
        /// <summary>
        /// If set to true, actions defined in onFinish will not be executed.
        /// <remarks>
        /// Optional parameter.
        /// Valid values are "try" or "false"
        /// </remarks>
        /// </summary>
        public string test { get; set; }
        /// <summary>
        /// Type of Request.
        /// <remarks>The only currently valid value is "file"</remarks>
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Credentials provided by Expensify for this account
        /// </summary>
        public Credentials credentials { get; set; }

        /// <summary>
        /// Default public constructor for the <seealso cref="ExpensifyJobRequest"/> class.
        /// </summary>
        public ExpensifyJobRequest()
        {
            credentials = new Credentials();
        }
    }

    /// <summary>
    /// This class represents a Download job request. It inherits from <seealso cref="ExpensifyJobRequest"/>
    /// and has a default <see cref="ExpensifyJobRequest.type"/> of "download".
    /// </summary>
    /// <remarks>See https://integrations.expensify.com/Integration-Server/doc/#downloader </remarks>
    public class ExpensifyDownloadJobRequest : ExpensifyJobRequest
    {
        /// <summary>
        /// The name of a file to download. Usually this is generated from an exporter job.
        /// </summary>
        public string fileName { get; set; }

        /// <summary>
        /// Default publich constructor for the <seealso cref="ExpensifyDownloadJobRequest"/>. 
        /// It will set the <see cref="ExpensifyJobRequest.type"/>job type to "download".
        /// </summary>
        public ExpensifyDownloadJobRequest()
        {
            type = "download";
        }
    }

    /// <summary>
    /// This class represents a job request to update the employee list. It inherits from <seealso cref="ExpensifyJobRequest"/>
    /// and has a default <see cref="ExpensifyJobRequest.type"/> of "update".
    /// </summary>
    /// <remarks>See https://integrations.expensify.com/Integration-Server/doc/#employee-updater </remarks>
    public class ExpensifyUpdateEmployeeJobRequest : ExpensifyJobRequest
    {
        /// <summary>
        /// The input settings used for an Employee Updater job request.
        /// See https://integrations.expensify.com/Integration-Server/doc/#update
        /// </summary>
        public UpdateEmployeeInputSettings inputSettings { get; set; }

        /// <summary>
        /// This is the default public constructor for <seealso cref="ExpensifyUpdateEmployeeJobRequest"/>.
        /// It sets the <see cref="ExpensifyJobRequest.type"/> to "update" and instantiates
        /// <see cref="inputSettings"/>.
        /// </summary>
        public ExpensifyUpdateEmployeeJobRequest()
        {
            type = "udpate";
            inputSettings = new UpdateEmployeeInputSettings();
        }
    }

    /// <summary>
    /// This class represents a Policy List Getter job request. It inherits from <seealso cref="ExpensifyJobRequest"/>
    /// and has a default <see cref="ExpensifyJobRequest.type"/> of "policyList".
    /// </summary>
    /// <remarks>See https://integrations.expensify.com/Integration-Server/doc/#policy-list-getter </remarks>
    public class ExpensifyGetPolicyListJobRequest : ExpensifyJobRequest
    {
        /// <summary>
        /// The input settings used for an Policy List Getter job request.
        /// See https://integrations.expensify.com/Integration-Server/doc/#policy-list-getter
        /// </summary>
        public GetPolicyListInputSettings inputSettings { get; set; }
        /// <summary>
        /// This is the default public constructor for <seealso cref="ExpensifyGetPolicyListJobRequest"/>.
        /// It sets the <see cref="ExpensifyJobRequest.type"/> to get and the 
        /// inputSettings.type to "policyList".
        /// </summary>
        public ExpensifyGetPolicyListJobRequest()
        {
            type = "get";
            inputSettings = new GetPolicyListInputSettings() { type = "policyList" };
        }
    }

    /// <summary>
    /// This class represents a Policy Getter job request. It inherits from <seealso cref="ExpensifyJobRequest"/>
    /// and has a default <see cref="ExpensifyJobRequest.type"/> of "policy".
    /// </summary>
    /// <remarks>See https://integrations.expensify.com/Integration-Server/doc/#policy-getter </remarks>
    public class ExpensifyGetPolicyJobRequest : ExpensifyJobRequest
    {
        /// <summary>
        /// The input settings used for an Policy Getter job request.
        /// See https://integrations.expensify.com/Integration-Server/doc/#policy-getter
        /// </summary>
        public GetPolicyInputSettings inputSettings { get; set; }
        /// <summary>
        /// This is the default public constructor for <seealso cref="ExpensifyGetPolicyJobRequest"/>.
        /// It sets the <see cref="ExpensifyJobRequest.type"/> to "get" and the
        /// inputSettings.type to "policy".
        /// </summary>
        public ExpensifyGetPolicyJobRequest()
        {
            type = "get";
            inputSettings = new GetPolicyInputSettings() { type = "policy" };
        }
    }

    /// <summary>
    /// This class represents a Policy Creator job request. It inherits from <seealso cref="ExpensifyJobRequest"/>
    /// and has a default <see cref="ExpensifyJobRequest.type"/> of "policy".
    /// </summary>
    /// <remarks>See https://integrations.expensify.com/Integration-Server/doc/#policy-creator </remarks>
    public class ExpensifyCreatePolicyJobRequest : ExpensifyJobRequest
    {
        /// <summary>
        /// Settings used to generate the policy.
        /// </summary>
        public CreatePolicyInputSettings inputSettings { get; set; }
        /// <summary>
        /// This is the default public constructor for <seealso cref="ExpensifyCreatePolicyJobRequest"/>.
        /// It sets the <see cref="ExpensifyJobRequest.type"/> to "policy" and instantiates
        /// <see cref="inputSettings"/>.
        /// </summary>
        public ExpensifyCreatePolicyJobRequest()
        {
            type = "create";
            inputSettings = new CreatePolicyInputSettings();
        }
    }

    /// <summary>
    /// This class represents a Policy Updater job request. It inherits from <seealso cref="ExpensifyJobRequest"/>
    /// and has a default <see cref="ExpensifyJobRequest.type"/> of "update".
    /// </summary>
    /// <remarks>See https://integrations.expensify.com/Integration-Server/doc/#policy-updater </remarks>
    public class ExpensifyUpdatePolicyJobRequest : ExpensifyJobRequest
    {
        /// <summary>
        /// The input settings <seealso cref="UpdatePolicyInputSettings"/>used to update the policy
        /// </summary>
        public UpdatePolicyInputSettings inputSettings { get; set; }
        
        /// <summary>
        /// List of <seealso cref="Categories"/> used to replace or update the existing categories of the policy
        /// </summary>
        public Categories categories { get; set; }
        
        /// <summary>
        /// List of <seealso cref="Tags"/> used to replace or update the existing tags of a policy
        /// </summary>
        public Tags tags { get; set; }
        
        /// <summary>
        /// A list of <seealso cref="ReportFields"/> used to replace or update the existing
        /// report fields of a policy.
        /// </summary>
        public ReportFields reportFields { get; set; }

        /// <summary>
        /// The default public constructor for <seealso cref="ExpensifyUpdatePolicyJobRequest"/>.
        /// It sets the <see cref="ExpensifyJobRequest.type"/> to "update" and instantiates
        /// <see cref="inputSettings"/>, <see cref="categories"/>, <see cref="tags"/> & <see cref="reportFields"/>.
        /// </summary>
        public ExpensifyUpdatePolicyJobRequest()
        {
            type = "update";
            inputSettings = new UpdatePolicyInputSettings();
            categories = new Categories();
            tags = new Tags();
            reportFields = new ReportFields();
        }
    }


    /// <summary>
    /// This class represents a Report Exporter job request. It inherits from <seealso cref="ExpensifyJobRequest"/>
    /// and has a default <see cref="ExpensifyJobRequest.type"/> of "file".
    /// </summary>
    /// <remarks>See https://integrations.expensify.com/Integration-Server/doc/#report-exporter </remarks>
    public class ExpensifyExportReportJobRequest : ExpensifyJobRequest
    {
        /// <summary>
        /// Action to be performed when the request is received.
        /// </summary>
        public Onreceive onReceive { get; set; }
        /// <summary>
        /// Settings used to filter the reports that are exported.
        /// </summary>
        public ReportExportInputsettings inputSettings { get; set; }
        /// <summary>
        /// Settings for the generated file.
        /// </summary>
        public Outputsettings outputSettings { get; set; }
        /// <summary>
        /// Maximum number of reports to export.
        /// <remarks>
        /// Optional parameter.
        /// Integer values in string format.</remarks>
        /// </summary>
        public string limit { get; set; }
        /// <summary>
        /// Actions performed at the end of the export.
        /// <remarks>
        /// Optional parameter.
        /// </remarks>
        /// </summary>
        public Onfinish[] onFinish { get; set; }
        
        /// <summary>
        /// The default public constructor for <seealso cref="ExpensifyExportReportJobRequest"/>.
        /// It sets the <see cref="ExpensifyJobRequest.type"/> to "update" and instantiates
        /// <see cref="inputSettings"/>, <see cref="onReceive"/>, <see cref="outputSettings"/> & <see cref="onFinish"/>.
        /// </summary>
        public ExpensifyExportReportJobRequest()
        {
            type = "file";
            onReceive = new Onreceive();
            inputSettings = new ReportExportInputsettings();
            outputSettings = new Outputsettings();
            onFinish = new Onfinish[0];
        }
    }

    public class ExpensifyUpdateReportStatusJobRequest : ExpensifyJobRequest
    {
        /// <summary>
        /// Input settings used to indicate the job type and provide filters.
        /// </summary>
        public UpdateReportInputsettings inputSettings { get; set; }

        /// <summary>
        /// Public default constructor.
        /// </summary>
        /// <remarks>
        /// Initializes type to "update" and instantiates inputSettings;
        /// </remarks>
        public ExpensifyUpdateReportStatusJobRequest()
        {
            type = "update";
            inputSettings = new UpdateReportInputsettings();
        }
    }

    //todo: need to add the following job requests:
    // Reconciliation
    // Expense Report Creator
    // Expense Creator
    // Expense Rules Updater

    /// <summary>
    /// This class represents the credentials used to make API calls to expensify.
    /// </summary>
    public class Credentials
    {
        /// <summary>
        /// Partner user ID generated by Expensify.
        /// </summary>
        public string partnerUserID { get; set; }
        /// <summary>
        /// A partner user secret generated by Expensify. To generate partner user IDs and secrets.
        /// </summary>
        public string partnerUserSecret { get; set; }
        /// <summary>
        /// Default constructor.
        /// <remarks>Automatically populates with Stoneridge credentials</remarks>
        /// </summary>
        public Credentials()
        {

        }
    }

    /// <summary>
    /// Class that holds parameters that specify the action to be 
    /// performed when the request is received.
    /// </summary>
    public class Onreceive
    {
        /// <summary>
        /// Actions to perform when the request is received.
        /// <remarks>Currently, the only valid value is "returnRandomFileName". So this value is assigned during construction.</remarks>
        /// </summary>
        public string[] immediateResponse { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <remarks>Assigns the only valid value for the immediateResponse actions</remarks>
        public Onreceive()
        {
            immediateResponse = new string[] { "returnRandomFileName" };
        }
    }

    /// <summary>
    /// This class holds the input paramters used in the <seealso cref="ExpensifyGetPolicyListJobRequest"/>
    /// </summary>
    public class GetPolicyListInputSettings
    {
        /// <summary>
        /// Specifies to the job that it has to get a policy summary list.
        /// </summary>
        /// <remarks>This value has to always be "policyList</remarks>
        public string type { get; set; }

        /// <summary>
        /// Whether or not to only get policies for which the user is an admin for.
        /// </summary>
        /// <remarks>
        /// Optional parameter.
        /// </remarks>
        public bool adminOnly { get; set; }

        /// <summary>
        /// Specifies the user to gather the policy list for.         
        /// </summary>
        /// <remarks>
        /// Optional parameter.
        /// You must have been granted third-party access by that user/company domain beforehand.
        /// </remarks>
        public string userEmail { get; set; }

        /// <summary>
        /// Default public constructor for <seealso cref="GetPolicyListInputSettings"/>
        /// </summary>
        /// <remarks>Assigns <see cref="GetPolicyListInputSettings.type"/> to "policyList"
        /// and <see cref="GetPolicyListInputSettings.adminOnly"/> to false.
        /// </remarks>
        public GetPolicyListInputSettings()
        {
            type = "policyList";
            adminOnly = false;
        }
    }

    /// <summary>
    /// This class holds the input paramters used in the <seealso cref="ExpensifyGetPolicyJobRequest"/>
    /// </summary>
    public class GetPolicyInputSettings
    {
        /// <summary>
        /// Specifies to the job that it has to get a policy.
        /// </summary>
        /// <remarks>This value has to always be "policy" so that is assigned in the constructor.</remarks>
        public string type { get; set; }
        /// <summary>
        /// The IDs of the policies for which to get information.
        /// </summary>
        public string[] policyIDList { get; set; }
        /// <summary>
        /// Specifies the fields of the policy for which to gather information.
        /// </summary>
        /// <remarks>
        /// The supported fields are Categories, reportFields, tags and tax information.
        /// So this field is filled out during construction with all fields.
        /// </remarks>
        public string[] fields { get; set; }

        /// <summary>
        /// Specifies the user for which to gather the policy data. 
        /// </summary>
        /// <remarks>
        /// You must have been granted third-party access by that user/company domain beforehand.
        /// </remarks>
        public string userEmail { get; set; }

        /// <summary>
        /// Default public constructor for <seealso cref="GetPolicyInputSettings"/>
        /// </summary>
        /// <remarks>Assigns <see cref="GetPolicyInputSettings.type"/> to "policy"
        /// and instantiates <see cref="GetPolicyInputSettings.fields"/> with 
        /// { "categories", "reportFields", "tags", "tax" }.
        /// </remarks>
        public GetPolicyInputSettings()
        {
            type = "policy";
            fields = new string[] { "categories", "reportFields", "tags", "tax" };
        }
    }

    /// <summary>
    /// This class holds the input paramters used in the <seealso cref="ExpensifyCreatePolicyJobRequest"/>
    /// </summary>
    public class CreatePolicyInputSettings
    {
        /// <summary>
        /// Specifies to the job that it has to create a policy.
        /// </summary>
        /// <remarks>
        /// Must be "policy".
        /// This value is set correctly in the default constructor.
        /// </remarks>
        public string type { get; set; }
        /// <summary>
        /// The name of the policy to create.
        /// </summary>
        public string policyName { get; set; }
        /// <summary>
        /// Specifies the plan of the policy. 
        /// </summary>
        /// <remarks>
        /// If not specified, the new policy will be created under the team plan.
        /// Valid values: “team”, “corporate”.
        /// The default value is not specified.
        /// </remarks>
        public string plan { get; set; }

        /// <summary>
        /// Default public constructor for <seealso cref="CreatePolicyInputSettings"/>
        /// </summary>
        /// <remarks>Assigns <see cref="CreatePolicyInputSettings.type"/> to "policy".
        /// </remarks>
        public CreatePolicyInputSettings()
        {
            type = "policy";
        }
    }

    /// <summary>
    /// This class is the base class used for all individual policy update job request.
    /// </summary>
    public class PolicyInputSettings
    {
        /// <summary>
        /// Specifies the type of job request.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// The identifier for the policy
        /// </summary>
        public string policyID { get; set; }
    }


    /// <summary>
    /// This class holds the input paramters used in the <seealso cref="ExpensifyUpdateEmployeeJobRequest"/>
    /// </summary>
    public class UpdateEmployeeInputSettings : PolicyInputSettings
    {
        /// <summary>
        /// The format of the file that contains the employee data.
        /// </summary>
        /// <remarks>
        /// Only CSV is supported at this point. This is the default value assigned by the constructor.
        /// </remarks>
        public string fileType { get; set; }

        /// <summary>
        /// Default public constructor for <seealso cref="UpdateEmployeeInputSettings"/>
        /// </summary>
        /// <remarks>
        /// Assigns <see cref="UpdateEmployeeInputSettings.type"/> to "employees"
        /// and <see cref="UpdateEmployeeInputSettings.fileType"/> to "csv".
        /// </remarks>
        public UpdateEmployeeInputSettings()
        {
            type = "employees";
            fileType = "csv";
        }
    }

    /// <summary>
    /// This class holds the input paramters used in the <seealso cref="ExpensifyUpdatePolicyJobRequest"/>
    /// </summary>
    public class UpdatePolicyInputSettings : PolicyInputSettings
    {
        /// <summary>
        /// Default public constructor for <seealso cref="UpdatePolicyInputSettings"/>
        /// </summary>
        /// <remarks>
        /// Assigns <see cref="UpdateEmployeeInputSettings.type"/> to "policy".
        /// </remarks>
        public UpdatePolicyInputSettings()
        {
            type = "policy";
        }
    }

    /// <summary>
    /// This class holds the input paramters used in the <seealso cref="ExpensifyExportReportJobRequest"/>
    /// </summary>
    public class ReportExportInputsettings
    {
        /// <summary>
        /// <remarks>Currently the only valid value is "combinedReportData" which will
        /// specify all Expensify reports will be combined into a single file.</remarks>
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Only the reports matching the specified status(es) will be exported.
        /// <remarks>
        /// Optional parameter.
        /// Valid values:
        ///     OPEN, SUBMITTED, APPROVED, REIMBURSED, ARCHIVED
        ///     Default value is Approved.
        /// When using multiple statuses, separate them by a comma.
        /// These values respectively match the statuses “Open”, “Processing”, “Approved”, 
        /// “Reimbursed” and “Closed” on the website.
        /// </remarks>
        /// </summary>
        public string reportState { get; set; }
        /// <summary>
        /// Parameter values used to filter reports that will be exported
        /// </summary>
        public ReportExportFilters filters { get; set; }

        /// <summary>
        /// Maximum number of reports to export.
        /// </summary>
        /// <remarks>
        /// Optional paramter.
        /// </remarks>
        public int limit { get; set; }
        /// <summary>
        /// The reports will be exported from that account.
        /// <remarks>
        /// Optional parameter.
        /// If this parameter is used, reports in the OPEN status cannot be exported
        /// </remarks>
        /// </summary>
        public string employeeEmail { get; set; }
        public ReportExportInputsettings()
        {
            type = "combinedReportData";
            reportState = "APPROVED";
            filters = new ReportExportFilters();
        }
    }

    /// <summary>
    /// This class holds the input paramters used in the <seealso cref="ExpensifyUpdateReportStatusJobRequest"/>
    /// </summary>
    public class UpdateReportInputsettings
    {
        /// <summary>
        /// Specifies to the job that it has to update the status of a list of reports.
        /// </summary>
        /// <remarks>Must be "reportStatus". Value is assigned during construction.</remarks>
        public string type { get; set; }

        /// <summary>
        /// The status to which to change the reports.
        /// </summary>
        /// <remarks>
        /// At the moment, only REIMBURSED is supported. So this value is assigned during construction.
        /// Only reports in the Approved status can be updated to Reimbursed.
        /// All other reports will be ignored.
        /// </remarks>
        public string status { get; set; }

        /// <summary>
        /// Parameter values used to filter reports that will be updated
        /// </summary>
        public Filters filters { get; set; }

        /// <summary>
        /// Public default constructor
        /// </summary>
        /// <remarks>
        /// Initializes type to "reportStatus" and status to "REIMBURSED".
        /// Instantiates filters.
        /// </remarks>
        public UpdateReportInputsettings()
        {
            type = "reportStatus";
            status = "REIMBURSED";
            filters = new Filters();
        }
    }

    /// <summary>
    /// Base class used for input settings filters
    /// </summary>
    /// <remarks>
    /// See also <seealso cref="ReportExportInputsettings.filters"/>
    /// </remarks>
    public class Filters
    {
        /// <summary>
        /// Comma-separated list of report IDs to be exported.
        /// </summary>
        public string reportIDList { get; set; }
        
        /// <summary>
        /// Filters all reports submitted or created before the given date, whichever occurred last (inclusive).
        /// <remarks>
        /// Required if reportIDList is not specified.
        /// Date format: yyy-mm-dd
        /// </remarks>
        /// </summary>
        public string startDate { get; set; }
        
        /// <summary>
        /// Filters all reports submitted or created after the given date, whichever occurred last (inclusive).
        /// <remarks>
        /// Optional Parameter
        /// Date format: yyy-mm-dd
        /// </remarks>
        /// </summary>
        public string endDate { get; set; }

    }

    /// <summary>
    /// Parameter values used to filter reports that will be exported
    /// </summary>
    public class ReportExportFilters : Filters
    {
        /// <summary>
        /// Comma-separated list of policy IDs the exported reports must be under.
        /// </summary>
        public string policyIDList { get; set; }

        /// <summary>
        /// Filters out all reports approved before, or on that date.
        /// </summary>
        /// <remarks>
        /// Optional Parameter
        /// Date format: yyy-mm-dd
        /// This filter is only used against reports that have been approved.
        /// </remarks>
        public string approvedAfter { get; set; }
        
        /// <summary>
        /// Filters out reports that have already been exported with that label.
        /// <remarks>
        /// Optional Parameter
        /// </remarks>
        /// </summary>
        public string markedAsExported { get; set; }

    }

    /// <summary>
    /// Parameter values used to specify information about the file to be exported
    /// </summary>
    public class Outputsettings
    {
        /// <summary>
        /// Specifies the format of the generated report. Note: if the “pdf” option is chosen, one PDF file will be generated for each report exported.
        /// <remarks>
        /// Valid values:
        /// “csv”, “xls”, "xlsx", “txt”, “pdf”, "json", "xml"
        /// </remarks>
        /// </summary>
        public string fileExtension { get; set; }
        /// <summary>
        /// The name of the generated file(s) will start with this value, and a random part will be added to make each filename globally unique. 
        /// If not specified, the default value export is used.
        /// <remarks>Optional Parameter</remarks>
        /// </summary>
        /// <remarks>
        /// Optional Parameter
        /// </remarks>
        public string fileBasename { get; set; }
        /// <summary>
        /// Specifies whether generated PDFs should include full page receipts.
        /// <remarks>
        /// Optional Parameter
        /// This parameter is used only if fileExtension contains pdf.
        /// </remarks>
        /// </summary>
        public bool includeFullPageReceiptsPdf { get; set; }

        /// <summary>
        /// Name of a workbook template stored for you on Expensify. (e.g. “testWorkbook.xlsx”)
        /// </summary>
        /// <remarks>
        /// Optional Parameter
        /// Specifies workbook template to which the report data will be written.
        /// If this is provided, then the only file made available to export will be an xlsx file.
        /// All other file extensions provided in fileExtension will be ignored.
        /// This is an enterprise feature.
        /// </remarks>
        public string spreadsheetFilename { get; set; }
    }


    /// <summary>
    /// Used to describe actions that need to be performed at the end of the export.
    /// </summary>
    public class Onfinish
    {
        /// <summary>
        /// Specifies the actions that need to be performed at the end of the export.
        /// </summary>
        /// <remarks>
        /// valid values are:
        /// <list type="string">
        /// <item "email"/><description "Send an email linking to the generated report to a provided list of recipients."/>
        /// <item "markAsExported"/><description "Mark the exported reports as “Exported” in Expensify."/>
        /// <item "sftpUpload"/><description "upload the generated file(s) to an SFTP server."/>
        /// </list>
        /// </remarks>
        public string actionName { get; set; }
    }


    /// <summary>
    /// Used to provide the necessary field values for the "email" finish action.
    /// </summary>
    public class EmailFinishAction : Onfinish
    {
        /// <summary>
        /// Default constructor
        /// <remarks> Sets actionName to "email"</remarks>
        /// </summary>
        public EmailFinishAction()
        { actionName = "email"; }
        /// <summary>
        /// Comma-separated list of valid email addresses to email at the end of the export.
        /// </summary>
        public string recipients { get; set; }
        /// <summary>
        /// Content of the message. 
        /// <remarks>
        /// Plain text or Freemarker message.
        /// If using Freemarker, the filenames list can be used to get the names of all files that have been generated.
        /// </remarks>
        /// </summary>
        public string message { get; set; }
    }

    public class MarkAsExportFinishAction : Onfinish
    {
        /// <summary>
        /// Default constructor
        /// <remarks> Sets actionName to "markAsExported"</remarks>
        /// </summary>
        public MarkAsExportFinishAction()
        {
            actionName = "markAsExported";
        }
        /// <summary>
        /// The exported reports will be marked as exported with this label.
        /// </summary>
        public string label { get; set; }
    }
    public class SftpUploadFinishAction : Onfinish
    {
    }

    /// <summary>
    /// The paramter data used to specify the SFTP upload data
    /// </summary>
    public class SftpUploadData
    {
        /// <summary>
        /// The SFTP host to which to connect.
        /// </summary>
        public string host { get; set; }

        /// <summary>
        /// The username to use for SFTP authentication.
        /// </summary>
        public string login { get; set; }

        /// <summary>
        /// The password to use during authentication.
        /// </summary>
        public string password { get; set; }

        /// <summary>
        /// The port to connect to on the SFTP server.
        /// </summary>
        public int port { get; set; }
    }


    /// <summary>
    /// This class is used to specify a list of categories to be used as expense categories in a policy.
    /// </summary>
    public class Categories
    {
        /// <summary>
        /// Specifies how the categories will be updated. 
        /// </summary>
        /// <remarks>
        /// - “replace” removes all existing categories and replaces them with the specified list
        /// - “merge” keeps existing categories and updates/adds the ones specified.
        /// </remarks>
        public string action { get; set; }

        /// <summary>
        /// An array of <seealso cref="CategoriesData"/> used to replace or update categories.
        /// </summary>
        public CategoriesData[] data { get; set; }
    }

    public class CategoriesData
    {
        /// <summary>
        /// The name of the category.
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Whether the category is enabled on Expensify.
        /// </summary>
        public bool enabled { get; set; }
        /// <summary>
        /// The GL Code associated to the category.
        /// </summary>
        /// <remarks>Optional Paramter</remarks>
        public string glCode { get; set; }
        /// <summary>
        /// The Payroll Code associated to the category.
        /// </summary>
        /// <remarks>Optional Paramter</remarks>
        public string payrollCode { get; set; }
        /// <summary>
        /// Whether comments are required for expenses under that category.
        /// </summary>
        /// <remarks>Optional Paramter</remarks>
        public bool areCommentsRequired { get; set; }
        /// <summary>
        /// The hint value for the comment for expenses under that category.
        /// </summary>
        /// <remarks>Optional Paramter</remarks>
        public string commentHint { get; set; }
        /// <summary>
        /// The maximum amount (in cents) for expenses under that category.
        /// </summary>
        /// <remarks>Optional Paramter</remarks>
        public int maxExpenseAmount { get; set; }
    }

    public class Tags
    {
        /// <summary>
        /// Provides the tags to be replaced in expensify
        /// </summary>
        /// <remarks>
        /// Multiple objects can be specified. Each one corresponds to a level in multi-level tagging.
        /// </remarks>
        public TagsData[] data { get; set; }
        public string source { get; set; }
        public Tags()
        {
            source = "inline";
        }
    }

    public class TagsData
    {
        /// <summary>
        /// The name of the tag level.
        /// </summary>
        /// <remarks>
        /// Only use with multi-level tagging.
        /// </remarks>
        public string name { get; set; }

        /// <summary>
        /// Specifies whether or not users must provide a tag value for that level
        /// when coding expenses.
        /// </summary>
        public bool setRequired { get; set; }

        /// <summary>
        /// The tags for this level.
        /// </summary>
        public Tag[] tags { get; set; }
    }

    public class Tag
    {
        /// <summary>
        /// The name of the tag.
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Whether the tag is enabled or not. Default value is true.
        /// </summary>
        /// <remarks>
        /// Note: When multi-level tagging is used, this value is ignored and considered true.
        /// </remarks>
        public bool enabled { get; set; }
        /// <summary>
        /// The GL (or other) Code associated to the tag.
        /// </summary>
        public string glCode { get; set; }
    }

    //Todo: Need to add the ability to use a csv file for tags.

    public class ReportFields
    {
        /// <summary>
        /// Specifies how the report fields will be updated. 
        /// </summary>
        /// <remarks>
        /// - “replace” removes all existing report fields and replaces them with the specified list
        /// - “merge” keeps existing report fields and updates/adds the ones specified.
        /// </remarks>
        public string action { get; set; }

        /// <summary>
        /// Provides the data used to replace or merge the report fields.
        /// </summary>
        /// <remarks><seealso cref="ReportFieldsData"/></remarks>
        public ReportFieldsData[] data { get; set; }

    }

    /// <summary>
    /// Provide the data used to replace or merge report fields.
    /// </summary>
    public class ReportFieldsData
    {
        /// <summary>
        /// The name of the report field.
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// The type of the report field.
        /// </summary>
        /// <remarks>
        /// “text”, “dropdown”, “date”
        /// </remarks>
        public string type { get; set; }
        /// <summary>
        /// The values of the dropdown.
        /// </summary>
        /// <remarks>
        /// Used only if type is “dropdown”
        /// </remarks>
        public string[] values { get; set; }

        /// <summary>
        /// The default value of the report field.
        /// </summary>
        /// <remarks>
        /// Optional parameter
        /// Only used for types “text” and “dropdown”
        /// </remarks>
        public string defaultValue { get; set; }
    }

    /// <summary>
    /// This class holds the parsed response value from <seealso cref="ExpensifyCreatePolicyJobRequest"/>
    /// </summary>
    public class CreatePolicyResponse
    {
        public int responseCode { get; set; }
        public string policyID { get; set; }
        public string policyName { get; set; }
    }

    /// <summary>
    /// This class holds the parsed response values from <seealso cref="ExpensifyGetPolicyJobRequest"/>
    /// </summary>
    public class GetPolicyResponse
    {
        public int responseCode { get; set; }
        public PolicyInfo[] policyInfo { get; set; }
    }

    /// <summary>
    /// This class holds the parsed response values from <seealso cref="ExpensifyGetPolicyListJobRequest"/>
    /// </summary>
    public class GetPolicyListResponse
    {
        public PolicyListInfo[] policyList { get; set; }
        public int responseCode { get; set; }
    }

    /// <summary>
    /// This class holds the parsed response values from <seealso cref="ExpensifyUpdateReportStatusJobRequest"/>
    /// </summary>
    public class UpdateReportStatusResponse
    {
        public int responseCode { get; set; }
        public int[] reportIDs { get; set; }
        public int[] skippedReports { get; set; }
        public int[] failedReports { get; set; }
    }

    /// <summary>
    /// This class represents the status information returned for Skipped and Failed reports when
    /// <seealso cref="SSExpensifyAPI.MakeUpdateReportStatusRequest(ExpensifyUpdateReportStatusJobRequest)"/>
    /// returns status 207 "Partial success"
    /// </summary>
    public class UpdateReportPartialResponse
    {
        /// <summary>
        /// The reportID that was either skipped or failed to update.
        /// </summary>
        public int reportID { get; set; }

        /// <summary>
        /// The reason for the skip or failure.
        /// </summary>
        public string reason { get; set; }
    }

    /// <summary>
    /// Policy data returned by <seealso cref="ExpensifyGetPolicyListJobRequest"/>
    /// </summary>
    public class PolicyListInfo
    {
        public string outputCurrency { get; set; }
        public string owner { get; set; }
        public string role { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public string type { get; set; }
    }


    /// <summary>
    /// Policy data returned by <seealso cref="ExpensifyGetPolicyJobRequest"/>
    /// </summary>
    public class PolicyInfo
    {
        public string id { get; set; }
        public ReportFieldsData[] reportFields { get; set; }
        public CategoriesData[] categories { get; set; }
        public Tag[] tags { get; set; }
        public Tax tax { get; set; }
    }


    public class Tax
    {
        public string _default { get; set; }
        public Rate[] rates { get; set; }
        public string name { get; set; }
    }

    public class Rate
    {
        public int rate { get; set; }
        public string name { get; set; }
        public string rateID { get; set; }
    }

}
