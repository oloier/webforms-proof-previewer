Imports System.Data
Imports System.IO
Imports System.Data.SqlClient

Partial Class masterpage
    Inherits System.Web.UI.MasterPage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Redirect ASP.NET's form post address to use our nicely rewritten URL for every postback
        form.Action = Request.RawUrl
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender

        ' Add a global page title suffix
        Page.Header.Title += " | Warwick Publishing"

        ' Global reports with color code selections
        Dim urlReport As String = Request.Params("report")
        If urlReport <> "" Then
            Dim reportMessage As String = HttpUtility.UrlDecode(urlReport) ' dev.stripHtml(urlReport)
            Dim reportClass As String = "error"
            Dim currentPath As String = Path.GetFileName(Request.PhysicalPath)
            If InStr(urlReport, "::") Then
                Dim suc As Array = Split(urlReport, "::")
                Dim colorCode As String = suc(1)
                If colorCode = "1" Then reportClass = "success"
                If colorCode = "2" Then reportClass = "info"
                reportMessage = dev.stripHtml(suc(0).ToString)
            End If
            dev.throwReport(reportClass, reportMessage, reportMaster, currentPath & " redirect report", "report: " & reportMessage)
        End If

    End Sub

End Class