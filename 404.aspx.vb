Partial Class fourohfour
    Inherits System.Web.UI.Page
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.StatusCode = "404"
        Response.StatusDescription = "File not found"
    End Sub
End Class