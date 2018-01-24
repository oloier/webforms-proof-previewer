Imports System.IO

Partial Class siteactive
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    End Sub

    Protected Sub takeEmDown()
        Dim config As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~")
        config.AppSettings.Settings.Item("siteActive").Value = "false"
        config.Save()
    End Sub

    Protected Sub putEmUp()
        Dim config As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~")
        config.AppSettings.Settings.Item("siteActive").Value = "true"
        config.Save()
    End Sub

    Protected Sub enable_click(sender As Object, e As System.EventArgs) Handles enable.Click
        putEmUp()
    End Sub

    Protected Sub disable_click(sender As Object, e As System.EventArgs) Handles disable.Click
        takeEmDown()
    End Sub

End Class