Imports System.IO
Imports System.IO.Compression
Imports System.Data
Imports System.Data.SqlClient
Imports System.Xml
Imports System.Diagnostics

Partial Class proof_default
    Inherits System.Web.UI.Page
    Dim proof As New proof

    Protected ReadOnly Property salesOrder As String
        Get
            Dim sonum As String = salesorder_.Text
            'Dim makeNumeric As New System.Text.RegularExpressions.Regex("[^0-9]")
            'Dim soNumeric As String = makeNumeric.Replace(sonum, String.Empty)
            Return sonum
        End Get
    End Property

    Protected ReadOnly Property leadTime As String
        Get
            Dim leadDays As String = leadtime_.Text
            Dim makeNumeric As New System.Text.RegularExpressions.Regex("[^0-9]")
            Dim leadNumeric As String = makeNumeric.Replace(leadDays, String.Empty)
            Return leadNumeric
        End Get
    End Property

    Protected ReadOnly Property nextProofNum As Integer
        Get
            Return proof.getNextProofID(salesOrder)
        End Get
    End Property

    Protected ReadOnly Property accountName As String
        Get
            Return account_.Text
        End Get
    End Property

    ' Create the proof identifier with our values
    Protected ReadOnly Property nextProofIdentifier As String
        Get
            Return salesOrder & "-" & nextProofNum
        End Get
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            Dim uploadFieldCount(19) As Integer
            uploadList.DataSource = uploadFieldCount
            uploadList.DataBind()
        End If
    End Sub

    Protected Sub saveProof_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles saveProof.Click
        Dim anyError As Boolean = False
        Dim reportMessage As String = ""
        Dim createdProof As New DataTable
        Dim account As String = account_.Text
        Dim salesOrder As String = salesorder_.Text
        Dim purchaseOrder As String = purchaseorder_.Text
        Dim mailto As String = mailto_.Text
        Dim comments As String = utils.stripNonAscii(comments_.Text)

        ' Reset the form validation for each time we submit
        accountBox.Attributes.Remove("class")
        salesorderBox.Attributes.Remove("class")
        proofBox.Attributes.Remove("class")
        mailtoBox.Attributes.Remove("class")
        report.Visible = False

        ' Validate our inputs and required fields
        If account = "" Then
            anyError = True
            reportMessage = "Please fill in required fields. "
            accountBox.Attributes.Add("class", "error")
        End If
        If salesOrder = "" Then
            anyError = True
            reportMessage = "Please fill in required fields. "
            salesorderBox.Attributes.Add("class", "error")
        End If
        'If leadTime = "" Then
        '    anyError = True
        '    reportMessage = "Please fill in required fields. "
        '    leadTimeBox.Attributes.Add("class", "error")
        'End If
        'If Not pdfFile.HasFile Then
        '    anyError = True
        '    reportMessage = "Please fill in required fields. "
        '    proofBox.Attributes.Add("class", "error")
        'End If
        Dim pdfCount As Integer = 0
        For Each item As RepeaterItem In uploadList.Items
            Dim pdfUpload As FileUpload = item.FindControl("pdfUpload")
            If pdfUpload.HasFile Then pdfCount += 1
        Next
        If pdfCount < 1 Then
            anyError = True
            reportMessage = "Please fill in required fields. "
            proofBox.Attributes.Add("class", "error")
        End If

        If mailto = "" Or Not utils.validEmail(mailto) Then
            anyError = True
            reportMessage = "Please fill in required fields. "
            mailtoBox.Attributes.Add("class", "error")
        End If

        ' Make sure we get all the info we need
        If Not anyError Then
            ' Increment our layouts
            Dim layoutCount As Integer = 0

            ' Let's store the JPG and convert before we insert the record to the db
            Dim imageList As New List(Of String)
            For Each item As RepeaterItem In uploadList.Items
                Dim pdfUpload As FileUpload = item.FindControl("pdfUpload")
                Dim zaeUpload As FileUpload = item.FindControl("zaeUpload")
                Dim fileLabel As TextBox = item.FindControl("fileLabel")
                Dim pdfFile As HttpPostedFile = pdfUpload.PostedFile
                Dim pdfName As String = pdfFile.FileName
                Dim zaeFile As HttpPostedFile = zaeUpload.PostedFile
                Dim zaeName As String = zaeFile.FileName
                Dim proofContentUrl As String = String.Format("../../proof-content.aspx?proof={0}&amp;layout={1}&amp;file=", nextProofIdentifier, item.ItemIndex + 1)
                Dim relativeProofPath As String = String.Format(".\proofs\{0}\layout{1}\", nextProofIdentifier, item.ItemIndex + 1)
                Dim proofSaveDir As String = Server.MapPath(relativeProofPath)
                If pdfUpload.HasFile Or zaeUpload.HasFile Then
                    Try
                        saveProofUpload(zaeFile, proofSaveDir)
                        saveProofUpload(pdfFile, proofSaveDir)
                        layoutCount += 1
                        Dim jpgCount As Integer = getFileCount("jpg", proofSaveDir)
                        imageList.Add(jpgCount & If(zaeUpload.HasFile, ",1", ",0"))
                        If zaeUpload.HasFile Then updateDaePaths(relativeProofPath, proofContentUrl)
                    Catch ex As Exception
                        dev.reportError(ex.Message, report, "")
                    End Try
                End If
            Next
            ' Concat our array into an image string
            Dim layoutScheme = String.Join("|", imageList)

            ' Connect to the database and fill in our values from the form
            Dim orderInsertId As Integer = 0
            Dim leadTimeObj As Object = leadTime
            If leadTime = "" Or leadTime = "0" Then leadTimeObj = DBNull.Value
            Dim dateCreated As DateTime = Date.Now

            Dim sql As String = "INSERT INTO [apps].[dbo].[proofs] "
            sql += "( [account]"
            sql += ", [salesOrder]"
            sql += ", [purchaseOrder]"
            sql += ", [proofNum]"
            sql += ", [layoutCount]"
            sql += ", [layoutScheme]"
            sql += ", [leadTime]"
            sql += ", [mailto]"
            sql += ", [blindProof]"
            sql += ", [dateCreated]"
            sql += ", [comments])"
            sql += " VALUES "
            sql += "( @account"
            sql += ", @salesOrder"
            sql += ", @purchaseOrder"
            sql += ", @proofNum"
            sql += ", @layoutCount"
            sql += ", @layoutScheme"
            sql += ", @leadTime"
            sql += ", @mailto"
            sql += ", @blindProof"
            sql += ", @dateCreated"
            sql += ", @comments );"

            Dim params As New Dictionary(Of String, Object) From {
                {"@account", account.ToString},
                {"@salesOrder", salesOrder.ToString},
                {"@purchaseOrder", purchaseOrder.ToString},
                {"@proofNum", nextProofNum},
                {"@layoutCount", layoutCount},
                {"@layoutScheme", layoutScheme},
                {"@leadTime", leadTimeObj},
                {"@mailto", mailto.ToString},
                {"@blindProof", IIf(blindProof.Checked, "1", "0")},
                {"@dateCreated", dateCreated.ToString},
                {"@comments", comments.ToString}
            }
            Dim newProof As Object
            Using datab As New dba(dev.dbcon, sql)
                newProof = datab.executeScalar(params)
            End Using

            Dim newProofId As Integer = 0 : Int32.TryParse(newProof.ToString, newProofId)
            If newProofId > 0 Then
                ' Email notification to the customer and link them to proof
                proof.sendCustomerProof(orderInsertId)

                reportMessage = "Proof created and sent, hooray."
                dev.reportSuccess(reportMessage, report, "proof creation")
                clearFormFields()
            Else
                dev.reportError(reportMessage, report, "proof creation")
            End If

        End If
    End Sub

    Protected Sub saveProofUpload(ByVal fileToUpload As HttpPostedFile, ByVal proofSaveDir As String)
        Dim fileName As String = fileToUpload.FileName
        Dim fileExtension As String = "pdf"
        Dim allowedExtensions As New Regex(".pdf|.zae") ' KEEP IT DRY
        Dim zaeUploaded As Boolean = False

        ' If the uploaded filename contains any of our allowed file extensions
        If fileToUpload.FileName <> "" AndAlso fileToUpload.ContentLength > 0 _
                AndAlso allowedExtensions.IsMatch(fileName.ToLower) Then

            If fileName.Contains("zae") Then
                fileExtension = "zip"
                zaeUploaded = True
            End If

            ' filename: salesorder#-revision_[i].ext
            Dim renamedFileName As String = String.Format("{0}.{1}", nextProofIdentifier, fileExtension)

            Try ' Save the upload since we know it's a ZAE or PDF
                saveFile(fileToUpload, renamedFileName, proofSaveDir)
            Catch ex As Exception
                Throw New Exception("Upload failed - " & ex.Message)
            End Try

            If zaeUploaded Then
                Try ' We know a ZAE is just a zip, so rename so .NET can extract
                    unzipArchive(renamedFileName, proofSaveDir)
                Catch ex As Exception
                    Throw New Exception("Unzip failed - " & ex.Message)
                End Try
            Else
                Try
                    convertPdfToJpg(renamedFileName, proofSaveDir)
                Catch ex As Exception
                    Throw New Exception("PDF conversion failed, check Ghostscript and ImageMagick installs - " & ex.Message)
                End Try
            End If

        End If
    End Sub

    Protected Sub updateDaePaths(ByVal relativeProofPath As String, ByVal proofContentUrl As String)
        Dim proofDir As String = Server.MapPath(relativeProofPath)
        ' rename the .dae to something consistent, dammit
        Dim command As Process = New Process()
        Try
            command.StartInfo.FileName = "cmd.exe"
            command.StartInfo.Arguments = String.Format(" /C REN {0}\*.dae model.dae", proofDir)
            command.StartInfo.UseShellExecute = False
            command.StartInfo.RedirectStandardOutput = False
            command.Start()
            'Dim stdout As String = command.StandardOutput.ReadToEnd()
            command.WaitForExit(3000)
        Catch ex As Exception
            dev.log(ex.Message)
        End Try

        ' Update the texture paths in XML document
        Dim xmlPath As String = proofDir & "model.dae"
        Dim xmlDoc As New XmlDocument
        xmlDoc.Load(xmlPath)
        Dim textures As XmlNodeList = xmlDoc.GetElementsByTagName("image")
        For Each node As XmlNode In textures
            Dim texturePath As String = node.FirstChild.InnerXml.ToString
            Dim newPath As String = String.Format(proofContentUrl & "Texture_")
            Try
                node.FirstChild.InnerXml = texturePath.Replace("./Texture_", newPath)
            Catch ex As Exception
                dev.log(ex.Message)
                Throw New Exception("XML failure - path text replace failed, check entities. " & ex.Message)
            End Try
        Next
        Try
            xmlDoc.Save(xmlPath)
        Catch ex As Exception
            dev.log(ex.Message)
            Throw New Exception("XML save failure - " & ex.Message)
        End Try
    End Sub

    Protected Sub saveFile(ByVal fileToUpload As HttpPostedFile, ByVal fileName As String, ByVal savePath As String)
        Try
            If Not Directory.Exists(savePath) Then
                Directory.CreateDirectory(savePath)
            End If
            fileToUpload.SaveAs((savePath & fileName))
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Protected Function getFileCount(ByVal fileType As String, ByVal directory As String) As Integer
        Dim pageCount As Integer = 0
        Dim command As Process = New Process()
        Try
            command.StartInfo.FileName = "cmd.exe"
            command.StartInfo.Arguments = String.Format(" /C DIR {0}\*.{1} /B", directory, "jpg")
            command.StartInfo.UseShellExecute = False
            command.StartInfo.RedirectStandardOutput = True
            command.Start()
            Dim stdout As String = command.StandardOutput.ReadToEnd()
            pageCount = stdout.Split(vbCrLf).Length - 1
            command.WaitForExit(3000)
        Catch ex As Exception
            dev.log(ex.Message)
            Throw ex
        End Try
        Return pageCount
    End Function

    Protected Sub convertPdfToJpg(ByVal inputFile As String, ByVal targetDir As String)
        Dim exePath As String = Server.MapPath(".\utils\ImageMagick-6.8.9-6\convert.exe")
        Dim resizeArgs As String = String.Format("-density 400 ""{0}{1}"" -resize 25% -quality 95 -background white -alpha remove -verbose ""{0}preview-%02d.jpg""", targetDir, inputFile)
        Dim pageCount As String = ""
        Dim convertPDF As Process = New Process()
        Try
            convertPDF.StartInfo.FileName = exePath
            convertPDF.StartInfo.Arguments = resizeArgs
            convertPDF.StartInfo.UseShellExecute = False
            convertPDF.StartInfo.RedirectStandardError = True
            convertPDF.Start()
            'Dim stdErr As String = convertPDF.StandardError.ReadToEnd()
            'pageCount = stdErr.Split(vbCrLf).Length - 1
            'dev.log(stdErr)
            convertPDF.WaitForExit()
        Catch ex As Exception
            dev.log(ex.Message)
            Throw ex
        End Try
    End Sub

    Public Sub unzipArchive(ByVal fileName As String, ByVal extractPath As String)
        Try
            ZipFile.ExtractToDirectory(extractPath & fileName, extractPath)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Protected Sub clearFormFields()
        account_.SelectedIndex = 0
        blindProof.Checked = False
        salesorder_.Text = String.Empty
        purchaseorder_.Text = String.Empty
        leadtime_.Text = String.Empty
        mailto_.Text = String.Empty
        comments_.Text = String.Empty
    End Sub


End Class
