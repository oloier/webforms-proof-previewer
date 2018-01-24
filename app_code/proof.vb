Imports System.Data
Imports System.Data.SqlClient

Public Class proof

    Public Function getNextProofID(ByVal salesOrder As Integer) As Integer
        Dim params As New Dictionary(Of String, Object) From {{"@orderNum", salesOrder}}
        Dim sql As String = "SELECT * FROM [apps].[dbo].[proofs] WHERE [salesOrder] = @orderNum"
        Using datab As New dba(dev.dbcon, sql)
            Dim proofData As DataTable = datab.readerDatatable(params)
            Return proofData.Rows.Count + 1
        End Using
    End Function

    Public Function getProofData(ByVal salesOrder As String, ByVal proofIdentifier As String) As DataTable
        Dim sql As String = "SELECT * FROM [apps].[dbo].[proofs] WHERE "
        sql += "[salesOrder] = @salesOrder AND [proofNum] = @proofNum"
        Dim params As New Dictionary(Of String, Object) From {
            {"@salesOrder", salesOrder},
            {"@proofNum", proofIdentifier}
        }
        Using datab As New dba(dev.dbcon, sql)
            Dim data As DataTable = datab.readerDatatable(params)
            Return data
        End Using
    End Function

    Public Function getProofData(ByVal proofid As Integer) As DataTable
        Dim sql As String = "SELECT * FROM [apps].[dbo].[proofs] WHERE [id] = @id"
        Dim params As New Dictionary(Of String, Object) From {{"@id", proofid}}
        Using datab As New dba(dev.dbcon, sql)
            Dim proofData As DataTable = datab.readerDatatable(params)
            Return proofData
        End Using
    End Function

    Public Function formatAccountToDomain(ByVal accountName As String, Optional blindProof As Boolean = False) As String
        If accountName = "Warwick Publishing" Or blindProof Then accountName = "wpcproofs"
        Return accountName.Replace(" ", "") & ".com"
    End Function

    Public Sub sendCustomerProof(ByVal proofid As Integer)
        Dim proofData As DataTable = Me.getProofData(proofid)
        If proofData.Rows.Count > 0 Then
            Dim proofRow As DataRow = proofData.Rows(0)
            Dim leadTime As String = proofRow("leadTime").ToString
            Dim accountName As String = proofRow("account").ToString
            Dim mailTo As String = proofRow("mailTo").ToString
            Dim comments As String = proofRow("comments").ToString
            Dim purchaseOrder As String = proofRow("purchaseOrder").ToString
            Dim salesOrder As String = proofRow("salesOrder").ToString
            Dim proofNum As String = proofRow("proofNum").ToString
            Dim proofIdentifier As String = String.Format("{0}-{1}", salesOrder, proofNum)
            Dim blindProof As Boolean = False
            If proofRow("blindProof").ToString = "1" Then blindProof = True
            Dim emailMessage As String = ""

            ' Color the big dumb text in the email, stupid
            Dim accentColor As String = "#000"
            Select Case accountName
                Case "Studio Style"
                    accentColor = "#5ca2a1"
                Case "On The Ball Promotions"
                    accentColor = "#32903F"
                Case "Easy Pocket Folders"
                    accentColor = "#18C"
                    'Case "Warwick Publishing"
                    '   colorCode = "#0054A4"
            End Select

            ' Grab site domain from account, but accommodate blind proofs
            Dim siteDomain As String = Me.formatAccountToDomain(accountName, blindProof)

            ' If we have a PO, show that in the subject for customers instead of SO
            Dim orderNumber As String = salesOrder
            If purchaseOrder <> "" Then
                orderNumber = purchaseOrder
            End If

            ' Pull out any branding specific stuff for Warwick
            Dim subjectPrefix As String = If(blindProof, "", String.Format("{0} - ", siteDomain)) ' strip domain prefix if blind
            Dim emailSubject As String = String.Format("{0}Your proof for order {1} Is ready!", subjectPrefix, orderNumber)

            ' Send user a proof approval notification email
            Dim emailExternal As New Email
            mailTo = Regex.Replace(mailTo, ",(?!\s)", ", ")
            emailExternal.ToEmail = mailTo
            emailExternal.Subject = emailSubject
            If Not blindProof Then
            End If
            emailExternal.FromEmail = settings.proofEmailAccount & "@" & siteDomain

            ' Setup the HTML of the email itself
            emailMessage = String.Format("<h2 style=""color:{0}; font-size:18px"">Your proof Is ready!</h2>", accentColor)
            emailMessage += String.Format("<p>Your requested proof for <b>order {0}", orderNumber)
            If purchaseOrder <> "" Then emailMessage += String.Format(" (sales order {0})", salesOrder)
            emailMessage += "</b> Is ready to view.</p>"
            emailMessage += "<p>This proof shows how your imprint will appear on your order. <br />Review it carefully And reply back to this email with your approval Or requested changes.</p>"


            ' Make a URL / button for each layout in the submitted proof email
            Dim layoutNum As String = proofRow("layoutCount").ToString
            Dim layoutCount As Integer = 0
            Int32.TryParse(layoutNum, layoutCount)

            ' One 'view online proof' button per layout in the email
            For layout As Integer = 1 To layoutCount
                ' Blind proofs handled via domain
                Dim buttonText As String = "View Online Proof"
                Dim proofUrl As String = String.Format("https://www.{0}/proof.aspx?proof={1}&amp;layout={2}", siteDomain, proofIdentifier, layout)
                If layoutCount > 1 Then
                    buttonText = String.Format("View Your Proof - Layout {0}", layout)
                Else
                    buttonText = String.Format("View Your Proof", layout)
                End If
                emailMessage += String.Format("<table cellspacing=""0"" cellpadding=""0"" style=""margin-bottom:15px !important""><tr><td align=""center"" width=""250"" height=""40"" bgcolor=""#ffd700"" style=""-webkit-border-radius: 5px; -moz-border-radius: 5px; border-radius: 5px; color: #000; display: block;""><a href=""{0}"" style=""font-size:14px; font-weight: bold; font-family: sans-serif; text-decoration: none; line-height:40px; width:100%; display:inline-block""><span style=""color: #000"">{1}</span></a></td></tr></table><br/>", proofUrl, buttonText)
            Next

            ' Display notes from composing department outlining any details customers may need to know
            If comments <> "" Then
                emailMessage += String.Format("<p><b>Additional Notes:</b><br/>{0}</p>", comments)
            End If

            ' Add the approval / changes / disclaimer text
            emailMessage += String.Format("<h2 style=""color:{0}; font-size:18px"">How to Proceed</h2>", accentColor)
            emailMessage += "<p><b>To approve this proof:</b> Reply to this email with your approval, name, And approval date And we will respond that it has been received.</p>"
            emailMessage += "<p><b>To request changes:</b> Reply with specific instructions. Unless otherwise specified, we will send a reproof reflecting those changes.</p>"
            emailMessage += "<p style=""font-size:15px"">★ <b> Your order Is on hold until we receive your reply.</b> "
            ' If we entered a lead time, lets format it to US and add to email
            If leadTime <> "" Then
                emailMessage += String.Format("Your order will be ready to ship <b>{0} business days from final approval.</b></p>", leadTime)
            End If
            emailMessage += "</p>"
            emailMessage += "<div style=""background:#f0f0f0; padding:12px; border-radius:5px; font-size:12px; color:#333;""><b>DISCLAIMER</b>"
            emailMessage += "<p style=""font-size:10px; color:#888"">The proof link shows how your imprint will appear on your order. Reply to this email with changes Or approval. This link includes a layout PDF, And a 3D view of your order.<br><br>Please make sure to check spelling, layout, And product to ensure your proof Is correct before submitting approval.<br><br>Since colors vary from monitor to monitor, this proof cannot be used to gauge exact color accuracy. One color imprints are shown in black &amp; white. Product outlines will Not print.</p></div>"


            ' Wrap the email in our shared HTML header/footer stuff
            emailExternal.Message = Me.buildProofEmail(accountName, emailMessage, blindProof)
            emailExternal.HtmlEncoded = True

            ' Send it outward
            emailExternal.sendEmail()
        End If
    End Sub

    Public Sub sendCustomerApproval(ByVal proofid As Integer)
        Dim proofData As DataTable = Me.getProofData(proofid)
        If proofData.Rows.Count > 0 Then
            Dim proofRow As DataRow = proofData.Rows(0)
            Dim accountName As String = proofRow("account").ToString
            Dim mailTo As String = proofRow("mailTo").ToString
            Dim purchaseOrder As String = proofRow("purchaseOrder").ToString
            Dim salesOrder As String = proofRow("salesOrder").ToString
            Dim proofNum As String = proofRow("proofNum").ToString
            Dim proofIdentifier As String = String.Format("{0}-{1}", salesOrder, proofNum)
            Dim blindProof As Boolean = If(proofRow("blindProof").ToString = "1", True, False)
            Dim emailMessage As String = ""

            ' Color the big dumb text in the email, stupid
            Dim accentColor As String = "#000"
            Select Case accountName
                Case "Studio Style"
                    accentColor = "#5ca2a1"
                Case "On The Ball Promotions"
                    accentColor = "#32903F"
                Case "Easy Pocket Folders"
                    accentColor = "#18C"
                    'Case "Warwick Publishing"
                    '   colorCode = "#0054A4"
            End Select

            ' Grab site domain from account, but accommodate blind proofs
            Dim siteDomain As String = Me.formatAccountToDomain(accountName, blindProof)

            ' If we have a PO, show that in the subject for customers instead of SO
            Dim orderNumber As String = salesOrder
            If purchaseOrder <> "" Then
                orderNumber = purchaseOrder
            End If

            ' Pull out any branding specific stuff for Warwick
            Dim subjectPrefix As String = If(blindProof, String.Format("{0} - ", siteDomain), "") ' strip domain prefix if blind
            Dim emailSubject As String = String.Format("{0}Your approval for order {1} has been received", subjectPrefix, orderNumber)

            ' Send user a proof approval notification email
            Dim emailExternal As New Email
            mailTo = Regex.Replace(mailTo, ",(?!\s)", ", ")
            emailExternal.ToEmail = mailTo
            emailExternal.Subject = emailSubject
            If Not blindProof Then
            End If
            emailExternal.FromEmail = settings.proofEmailAccount & "@" & siteDomain

            ' Setup the HTML of the email itself
            emailMessage = String.Format("<h2 style=""color:{0}; font-size:18px"">Thank you!</h2>", accentColor)
            emailMessage += String.Format("<p>Your approval of order {0} has been received. We appreciate your prompt reply.</p>", orderNumber)
            emailMessage += "<p></p>"


            ' Make a URL / button for each layout in the submitted proof email
            Dim layoutNum As String = proofRow("layoutCount").ToString
            Dim layoutCount As Integer = 0
            Int32.TryParse(layoutNum, layoutCount)

            ' One 'view online proof' button per layout in the email
            'For layout As Integer = 1 To layoutCount
            '    ' Blind proofs handled via domain
            '    Dim buttonText As String = "View Online Proof"
            '    Dim proofUrl As String = String.Format("https://www.{0}/proof.aspx?proof={1}&amp;layout={2}", siteDomain, proofIdentifier, layout)
            '    If layoutCount > 1 Then
            '        buttonText = String.Format("View Your Proof - Layout {0}", layout)
            '    Else
            '        buttonText = String.Format("View Your Proof", layout)
            '    End If
            '    emailMessage += String.Format("<table cellspacing=""0"" cellpadding=""0"" style=""margin-bottom:15px !important""><tr><td align=""center"" width=""250"" height=""40"" bgcolor=""#ffd700"" style=""-webkit-border-radius: 5px; -moz-border-radius: 5px; border-radius: 5px; color: #000; display: block;""><a href=""{0}"" style=""font-size:14px; font-weight: bold; font-family: sans-serif; text-decoration: none; line-height:40px; width:100%; display:inline-block""><span style=""color: #000"">{1}</span></a></td></tr></table><br/>", proofUrl, buttonText)
            'Next

            ' Wrap the email in our shared HTML header/footer stuff
            emailExternal.Message = Me.buildProofEmail(accountName, emailMessage, blindProof)
            emailExternal.HtmlEncoded = True

            ' Send it outward
            emailExternal.sendEmail()
        End If
    End Sub


    Public Function buildProofEmail(ByVal accountName As String, ByVal messageContent As String, ByVal blindProof As Boolean) As String
        Dim emailString As String = "<html>"
        emailString += "<body leftmargin=0 marginwidth=0 topmargin=0 marginheight=0 offset=0 bgcolor=white >"
        emailString += "<div style=""padding:2.5%; font:13px sans-serif; color:#333; line-height:1.3em;"">"

        ' Add company header if we are not under Warwick
        If Not blindProof Then
            emailString += "<h1 style=""margin-top:0;font-size:24px"">" & accountName & "</h1>"
        End If

        ' Add the actual content (order number, button + url)
        emailString += messageContent

        ' Add footer stuff if we are not under Warwick
        If Not blindProof Then
            Dim phone As String = ""
            Dim address As String = "2601 E. Main Street<br/>Saint Charles, IL 60174-4289"

            If accountName = "Studio Style" Then
                phone = "Customer service: 800-346-3063"
            End If

            If accountName = "Easy Pocket Folders" Then
                phone = "Customer service: 800-346-3063"
            End If

            If accountName = "On The Ball Promotions" Then
                phone = "Customer service: 800-475-2255"
            End If

            emailString += String.Format("<p style=""margin-top:2em;font-size:11px; color:#555;""><b>{0}</b><br/>{1}<br/>{2}</p>", accountName, address, phone)
        End If

        emailString += "</div></body></html>"

        Return emailString
    End Function

    Public Function getOutstandingProofs(Optional accountName As String = "") As DataTable
        Dim sql As String = "SELECT * FROM [apps].[dbo].[proofs] WHERE [dateApproved] IS NULL"
        If accountName <> "" AndAlso accountName <> "Warwick Publishing" Then
                    sql += " And [account] <> 'Warwick Publishing'"
                ElseIf accountName = "Warwick Publishing" Then
                    sql += " AND [account] = 'Warwick Publishing'"
                End If
        Dim params As New Dictionary(Of String, Object) From {{"", ""}}
        Using datab As New dba(dev.dbcon, sql)
            Dim proofData As DataTable = datab.readerDatatable(params)
            Return proofData
        End Using

    End Function

End Class
