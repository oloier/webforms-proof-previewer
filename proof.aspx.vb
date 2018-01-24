Imports System.Data.SqlClient
Imports System.Data

Partial Class proof_view
    Inherits System.Web.UI.Page
    Protected modelPath As String

    Protected Function sanitizeParam(ByVal inputString As String) As String
        If inputString <> "" Then
            Return System.Text.RegularExpressions.Regex.Replace(inputString, "[^a-zA-Z0-9-_.]", "").ToLower
        Else
            Return ""
        End If
    End Function

    Protected Property purchaseOrder As String
    Protected Property approved As Boolean

    Dim proofNum As String = ""
    Protected Property proofVersion As String
        Get
            Return proofNum
        End Get
        Set(proofUrlArg As String)
            If proofUrlArg <> "" Then
                proofUrlArg = sanitizeParam(proofUrlArg)
                proofNum = proofUrlArg.Split("-")(1)
            End If
        End Set
    End Property

    Dim proofId As String = ""
    Protected Property salesOrder As String
        Get
            Return proofId
        End Get
        Set(proofUrlArg As String)
            If proofUrlArg <> "" Then
                proofUrlArg = sanitizeParam(proofUrlArg)
                proofId = proofUrlArg.Split("-")(0)
            End If
        End Set
    End Property

    Dim layoutInt As Integer = 0
    Protected Property layoutNum As Integer
        Get
            Return layoutInt
        End Get
        Set(layoutUrlArg As Integer)
            If layoutUrlArg > 0 Then
                layoutInt = layoutUrlArg
            End If
        End Set
    End Property

    Dim proofDataDT As DataTable
    Protected Property proofData As DataTable
        Get
            Return proofDataDT
        End Get
        Set(proofData As DataTable)
            If proofDataDT Is Nothing OrElse proofDataDT.Rows.Count = 0 Then
                proofDataDT = proofData
            End If
        End Set
    End Property

    Protected ReadOnly Property proofIdentifier As String
        Get
            Return salesOrder & "-" & proofVersion
        End Get
    End Property

    Dim domain As String = ""
    'Protected Property siteDomain As String
    '    Get
    '        Return domain
    '    End Get
    '    Set(accountName As String)
    '        If domain = "" Then
    '            Dim blind As Boolean = proofData.Rows(0).Item("blindProof").ToString
    '            If blind Then accountName = "wpcproofs"
    '            domain = LCase(accountName.Replace(" ", "") & ".com")
    '        End If
    '    End Set
    'End Property

    'Dim commentStr As String = ""
    'Protected Property commentsText As String
    '    Get
    '        Return commentStr
    '    End Get
    '    Set(comments As String)
    '        commentStr = comments
    '    End Set
    'End Property

    Dim layout As String = ""
    Protected Property layoutScheme As String
        Get
            Return layout
        End Get
        Set(value As String)
            If layoutNum > 0 Then
                layout = value.Split("|")(layoutNum - 1)
            End If
        End Set
    End Property

    Protected ReadOnly Property imageCount As Integer
        Get
            Dim countInt As Integer = 0
            If layoutScheme.Length > 0 Then
                Dim jpgArray As Array = layoutScheme.Split(",")
                Dim jpgCount As String = jpgArray(0).ToString
                Int32.TryParse(jpgCount, countInt)
            End If

            Return countInt - 1
        End Get
    End Property

    'Protected ReadOnly Property leadTime As Integer
    '    Get
    '        If proofData.Rows.Count > 0 Then
    '            Dim leadInt As Integer = 0
    '            Int32.TryParse(proofData.Rows(0).Item("leadtime").ToString, leadInt)
    '            Return leadInt
    '        Else
    '            Return 0
    '        End If
    '    End Get
    'End Property

    'Protected ReadOnly Property arriveDate As DateTime
    '    Get
    '        Dim dateViewed As DateTime = System.DateTime.Now
    '        Dim businessDays As Integer = addBusinessDays(dateViewed, leadTime)
    '        Dim newDate As DateTime = dateViewed.AddDays(businessDays)
    '        Return newDate
    '    End Get
    'End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Request.QueryString("proof") = "" Or Request.QueryString("layout") = "" Then
            Response.Redirect("default.aspx?report=You need a proof number in order to view it::2")
        Else
            ' Set our page properties
            proofVersion = Request.QueryString("proof")
            salesOrder = Request.QueryString("proof")
            Int32.TryParse(Request.QueryString("layout"), layoutInt)
            layoutNum = layoutInt
            proofData = getProofData(salesOrder, proofVersion)

            If proofData IsNot Nothing AndAlso proofData.Rows.Count > 0 Then
                ' Set more page properties via DB results
                'commentsText = proofData.Rows(0).Item("comments").ToString
                'siteDomain = proofData.Rows(0).Item("account").ToString
                purchaseOrder = proofData.Rows(0).Item("purchaseOrder").ToString
                layoutScheme = proofData.Rows(0).Item("layoutScheme").ToString
                Dim approvalTime As String = proofData.Rows(0).Item("dateApproved").ToString
                If approvalTime <> "" Then approved = True
                If approved Then
                    Response.Redirect("default.aspx?report=Your proof has already been approved::2")
                End If
                ' Setup the model
                If layoutScheme.Contains(",1") Then
                    modelPath = String.Format("./proof-content.aspx?proof={0}&layout={1}&file=model.dae", proofIdentifier, layoutNum)
                    gotoModel.Visible = True
                End If

                ' If we have a PO, prioritize displaying that, otherwise default to our SO
                Dim orderNum As String = salesOrder
                If purchaseOrder <> "" Then
                    orderNum = purchaseOrder
                End If
                Page.Title = "Proof for order " & orderNum


                ' Update DB with time viewed if not internal link
                If Request.QueryString("admin") <> "1" Then
                    updateViewDate()
                End If

                ' Lets show the proof comments here from now on too
                'If commentsText <> "" Then
                '    commentsBox.Visible = True
                '    comments.Text = commentsText
                'End If

                ' Display the SO or PO, depending on what we got
                proofNumber.Text = String.Format("{0} &ndash; Layout {1}", orderNum, layoutNum)

                ' Display the estimated ship date
                'estShipDate.Text = arriveDate.ToLongDateString

                ' Setup URL for PDF download
                proofDownload.HRef = String.Format("proof-content.aspx?proof={0}&layout={1}&file={0}.pdf", proofIdentifier, layoutNum)

                ' Iterate and bind image previews array
                If imageCount <> -1 Then
                    Dim intArr(imageCount) As Integer
                    ' Show our thumbnails when we've got more than 1 img
                    thumbs.DataSource = intArr
                    thumbs.DataBind()
                    thumbnails.Visible = True
                End If

            End If
        End If
    End Sub

    Protected Sub thumbs_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles thumbs.ItemDataBound
        If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim id As Integer = e.Item.ItemIndex
            Dim link As HtmlAnchor = e.Item.FindControl("thumbLink")
            Dim thumb As HtmlImage = e.Item.FindControl("thumbImg")
            Dim proofJpg As String = "preview-" & String.Format("{0:00}", id) & ".jpg"

            Dim url As String = String.Format("proof-content.aspx?proof={0}&layout={1}&file={2}", proofIdentifier, layoutNum, proofJpg)
            thumb.Src = url
            link.HRef = url
        End If
    End Sub

    Protected Sub updateViewDate()
        Dim dateViewed As DateTime = System.DateTime.Now
        Using con As New SqlConnection(dev.dbcon)
            Dim sql As String = "UPDATE [apps].[dbo].[proofs] SET" &
                                " [dateViewed] = @dateViewed" &
                                " WHERE [salesOrder] = @SO" &
                                " AND [proofNum] = @PN"
            Dim cmd As New SqlCommand(sql, con)
            With cmd.Parameters
                .Add(New SqlParameter("@dateViewed", dateViewed))
                .Add(New SqlParameter("@SO", salesOrder))
                .Add(New SqlParameter("@PN", proofVersion))
            End With
            Try
                con.Open()
                cmd.CommandText = sql
                cmd.ExecuteNonQuery()
            Catch ex As Exception
                dev.log(ex.Message)
            End Try
        End Using
    End Sub

    Protected Function addBusinessDays(ByVal startDate As DateTime, ByVal leadTime As Integer) As Integer
        If leadTime < (5 - startDate.DayOfWeek) Then
            Return leadTime
        Else
            'businessDays = leadTime + (Int(((leadTime + (7 - startDate.DayOfWeek)) / 5)) * 2)
            Dim weekendDays = 2 * Math.Floor((startDate.DayOfWeek + leadTime - 0.1) / 5)
            Return (leadTime + weekendDays)
        End If
    End Function

    Public Function getProofData(ByVal salesOrder As String, ByVal proofIdentifier As String) As DataTable
        Dim sql As String = "SELECT * FROM [apps].[dbo].[proofs] WHERE [salesOrder] = @salesOrder AND [proofNum] = @proofNum"
        Using datab As New dba(dev.dbcon, sql)
            Dim params As New Dictionary(Of String, Object) From {
                {"@salesOrder", salesOrder},
                {"@proofNum", proofIdentifier}
            }
            Return datab.readerDatatable(params)
        End Using
    End Function

End Class