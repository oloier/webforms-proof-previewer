Imports System.IO
Imports System.Data.SqlClient
Imports System.Data

Partial Class proof_list
    Inherits System.Web.UI.Page
    Dim proofy As New proof

    ReadOnly Property accountName As String
        Get
            If Request.QueryString("account") <> "" Then
                Dim acct As String = Request.QueryString("account")
                If acct = "warwick" Then
                    Return "Warwick Publishing"
                Else : Return "others"
                End If
            Else
                Return ""
            End If
        End Get
    End Property

    Protected Sub Page_PreRender(sender As Object, e As System.EventArgs) Handles Me.PreRender
        updateDatabind()
    End Sub

    Private Sub updateDatabind()
        Dim outstandingProofs As DataTable = proofy.getOutstandingProofs(accountName)
        If outstandingProofs.Rows.Count > 0 Then
            proofsList.DataSource = outstandingProofs
            proofsList.DataBind()
        End If
    End Sub

    Protected Sub proofsList_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles proofsList.ItemDataBound
        If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim row As DataRow = CType(e.Item.DataItem, DataRowView).Row

            Dim proofSqlId_ As String = row("id").ToString
            Dim proofSqlId As HiddenField = e.Item.FindControl("proofSqlId")
            proofSqlId.Value = proofSqlId_
            Dim accountName_ As String = row("account").ToString
            Dim salesOrder_ As String = row("salesOrder").ToString
            Dim proofSo As HiddenField = e.Item.FindControl("proofSo")
            proofSo.Value = salesOrder_
            Dim proofPo As HiddenField = e.Item.FindControl("proofPo")
            Dim purchaseOrder As String = row("purchaseOrder").ToString
            proofPo.Value = purchaseOrder
            Dim proofid_ As String = row("proofNum").ToString
            Dim title As HtmlGenericControl = e.Item.FindControl("title")
            Dim proofIdentifier As String = salesOrder_ & "-" & proofid_
            Dim proofidCarrier As HiddenField = e.Item.FindControl("proofid")
            proofidCarrier.Value = proofIdentifier
            Dim poCheck As String = If(purchaseOrder <> "", "<br>PO: " & purchaseOrder, "")
            title.InnerHtml = String.Format("{0} <span>SO: {1}{2}</span>", accountName_, proofIdentifier, poCheck)

            ' Show the blind proof icon for blind proofs
            Dim blindProof As Boolean = If(row("blindProof").ToString = "1", True, False)
            If blindProof Then
                title.Attributes.Add("data-icon-right", "")
            End If


            Dim dateViewed_ As String = row("dateViewed").ToString
            Dim dateDate As DateTime
            DateTime.TryParse(dateViewed_, dateDate)
            Dim dateViewed As HtmlGenericControl = e.Item.FindControl("dateViewed")
            If dateViewed_ <> "" Then
                dateViewed.InnerHtml = "Viewed on: <b>" & dateDate.ToString & "</b>"
            Else : dateViewed.InnerHtml = "<b>Never viewed</b>"
            End If

            Dim dateCreated_ As String = row("dateCreated").ToString
            DateTime.TryParse(dateCreated_, dateDate)
            Dim dateCreated As HtmlGenericControl = e.Item.FindControl("dateCreated")
            If dateCreated_ <> "" Then
                dateCreated.InnerHtml = "Sent on: <b>" & dateDate.ToString & "</b>"
            End If

            Dim layoutCountStr As String = row("layoutCount").ToString
            Dim layoutCount As Integer = 0
            Int32.TryParse(layoutCountStr, layoutCount)
            Dim viewsHtml As String = ""
            If layoutCount <> -1 Then
                Dim intArr(layoutCount - 1) As Integer
                For i As Integer = 0 To intArr.Length - 1
                    Dim layNum As Integer = i + 1 ' whole numbers for slightly more user friendly links?
                    Dim viewLink As String = String.Format("http://" & proofy.formatAccountToDomain(accountName_, blindProof) & "/proof.aspx?proof={0}&layout={1}&admin=1", proofIdentifier, layNum)
                    viewsHtml += String.Format("<a href=""{0}"" data-icon="""" rel=""external"">View layout {1}</a>", viewLink, layNum)
                Next
            End If
            ' Populate the container with our formatted links
            Dim viewLinks As HtmlGenericControl = e.Item.FindControl("viewLinks")
            viewLinks.InnerHtml = viewsHtml


            Dim mailto As HtmlGenericControl = e.Item.FindControl("mailto")
            Dim email As String = row("mailto").ToString
            mailto.InnerHtml = email

            Dim comments_ As String = row("comments").ToString
            Dim comments As HtmlGenericControl = e.Item.FindControl("comments")
            If comments_ <> "" Then
                comments.InnerHtml = comments_
                'comments.Visible = True
            End If

        End If
    End Sub

    Protected Sub sendmail(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim btn As LinkButton = CType(sender, LinkButton)
        Dim row As RepeaterItem = btn.NamingContainer
        Dim proofIdentifier As HiddenField = row.FindControl("proofSqlId")
        Dim report As HtmlGenericControl = row.FindControl("report")
        Dim proofid As Integer = 0
        Int32.TryParse(proofIdentifier.Value, proofid)

        Try
            proofy.sendCustomerProof(proofid)
            dev.reportSuccess("Proof re-sent!", report, "proof resending button click")
        Catch ex As Exception
            dev.log(ex.Message)
        End Try

    End Sub

    Protected Sub sendApproval(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim btn As LinkButton = CType(sender, LinkButton)
        Dim row As RepeaterItem = btn.NamingContainer
        'Dim report As HtmlGenericControl = row.FindControl("report")
        Dim proofSqlId As HiddenField = row.FindControl("proofSqlId")
        Dim proofid As HiddenField = row.FindControl("proofid")
        Dim proofNum As String = proofid.Value
        Dim proofSqlIdInt As Int32 : Int32.TryParse(proofSqlId.Value, proofSqlIdInt)

        Dim anyError As Boolean = False
        Dim reportMessage As String = ""
        'If approve.Checked Then
        '    reportMessage = "Thank you for submitting your approval."
        'Else
        '    reportMessage = "Thank you for submitting your corrections."
        'End If
        'Dim correctionText As String = dev.stripHtml(corrections.Text)

        ' Commit our changes to the database
        Try
            approveProof(proofSqlIdInt)
            proofy.sendCustomerApproval(proofSqlId.Value)
            reportMessage = String.Format("Proof for {0} was approved", proofNum)
            dev.reportSuccess(reportMessage, report, "proof approval button click")
        Catch ex As ApplicationException
            dev.log(ex.Message)
            dev.reportError(reportMessage, report, "proof approval button click")
        End Try
    End Sub

    Private Sub approveRemoveFiles(proofIdentifier As String)
        Dim path As String = String.Format("X:\inetpub\webapps\proofs\{0}", proofIdentifier)
        path = Server.MapPath(path)
        My.Computer.FileSystem.DeleteDirectory(path, FileIO.DeleteDirectoryOption.DeleteAllContents, FileIO.RecycleOption.SendToRecycleBin)
    End Sub


    Protected Sub delApprove(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim btn As LinkButton = CType(sender, LinkButton)
        Dim row As RepeaterItem = btn.NamingContainer
        'Dim report As HtmlGenericControl = row.FindControl("report")
        Dim proofSqlId As HiddenField = row.FindControl("proofSqlId")
        Dim proofid As HiddenField = row.FindControl("proofid")
        Dim proofNum As String = proofid.Value
        Dim proofSqlIdInt As Int32 : Int32.TryParse(proofSqlId.Value, proofSqlIdInt)

        Dim anyError As Boolean = False
        Dim reportMessage As String = ""

        ' Commit our changes to the database
        Try
            approveProof(proofSqlIdInt)
            reportMessage = String.Format("Proof {0} was removed successfully.", proofNum)
            dev.reportSuccess(reportMessage, report, "proof approval button click")
        Catch ex As ApplicationException
            dev.log(ex.Message)
            dev.reportError(reportMessage, report, "proof approval button click")
        End Try
    End Sub


    Private Sub approveProof(proofSqlId As Integer)
        Using con As New SqlConnection(dev.dbcon)
            Dim sql As String = "UPDATE [apps].[dbo].[proofs] SET" &
                                " [dateApproved] = @dateApproved" &
                                " WHERE [id] = @id"
            '",[reproof] = @reproof" &
            '",[chosenProof] = @chosenProof" &
            '",[corrections] = @corrections" &

            Dim cmd As New SqlCommand(sql, con)
            With cmd.Parameters
                Dim dateNow As DateTime = System.DateTime.Now
                .Add(New SqlParameter("@dateApproved", dateNow))
                '.Add(New SqlParameter("@dateApproved", SqlDateTime.Null))
                'If reproof.Checked Then
                '    .Add(New SqlParameter("@reproof", "1"))
                'Else
                '    .Add(New SqlParameter("@reproof", DBNull.Value))
                'End If
                'If Request.Params("choice_") <> "" Then
                '    .Add(New SqlParameter("@chosenProof", Request.Params("choice_")))
                'Else
                '    .Add(New SqlParameter("@chosenProof", DBNull.Value))
                'End If
                'If corrections.Text <> "" Then
                '    .Add(New SqlParameter("@corrections", correctionText))
                'Else
                '    .Add(New SqlParameter("@corrections", DBNull.Value))
                'End If
                .Add(New SqlParameter("@id", proofSqlId))
            End With
            Try
                con.Open()
                cmd.CommandText = sql
                cmd.ExecuteNonQuery()
                updateDatabind()
            Catch ex As Exception
                Throw New ApplicationException("There was a problem with your submission, please try again. If problem persists, please contact the web department.")
            End Try
        End Using
    End Sub

End Class
