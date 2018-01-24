Imports Microsoft.VisualBasic

Public Class Email
    Dim p_ToEmail As String = ""
    Dim p_FromEmail As String = ""
    Dim p_ReplyToEmail As String = ""
    Dim p_CCEmail As String = ""
    Dim p_Subject As String = ""
    Dim p_Message As String = ""
    Dim p_ErrorMessage As String = ""
    Dim p_HtmlEncoded As Boolean = True

    Public ReadOnly Property ErrorMessage() As String
        Get
            Return p_ErrorMessage
        End Get
    End Property
    Public Property ToEmail() As String
        Get
            Return p_ToEmail
        End Get
        Set(ByVal value As String)
            p_ToEmail = value
        End Set
    End Property
    Public Property FromEmail() As String
        Get
            Return p_FromEmail
        End Get
        Set(ByVal value As String)
            p_FromEmail = value
        End Set
    End Property
    Public Property ReplyToEmail() As String
        Get
            Return p_ReplyToEmail
        End Get
        Set(ByVal value As String)
            p_ReplyToEmail = value
        End Set
    End Property
    Public Property CCEmail() As String
        Get
            Return p_CCEmail
        End Get
        Set(ByVal value As String)
            p_CCEmail = value
        End Set
    End Property
    Public Property Subject() As String
        Get
            Return p_Subject
        End Get
        Set(ByVal value As String)
            p_Subject = value
        End Set
    End Property
    Public Property Message() As String
        Get
            Return p_Message
        End Get
        Set(ByVal value As String)
            p_Message = value
        End Set
    End Property
    Public Property HtmlEncoded() As Boolean
        Get
            Return p_HtmlEncoded
        End Get
        Set(ByVal value As Boolean)
            p_HtmlEncoded = value
        End Set
    End Property

    Public Function sendEmail() As Boolean
        sendEmail = False
        Try
            If p_FromEmail = "" Then
                p_ErrorMessage = "A from address is required"
                Exit Function

            End If
            If p_ToEmail = "" Then
                p_ErrorMessage = "A to address is required"
                Exit Function
            End If

            Dim mm As New System.Net.Mail.MailMessage(p_FromEmail, p_ToEmail)

            '(2) Assign the MailMessage's properties
            mm.Subject = p_Subject
            If p_CCEmail <> "" Then
                mm.CC.Add(p_CCEmail)
            End If
            mm.Body = p_Message
            mm.IsBodyHtml = p_HtmlEncoded

            'mm.Headers.Add("Reply-To", settings.emailFrom)

            Dim smtp As New System.Net.Mail.SmtpClient
            smtp.Send(mm)
            sendEmail = True
        Catch ex As Exception
            p_ErrorMessage = ex.Message
        End Try

    End Function

    Public Function sendEmailLocal() As Boolean
        sendEmailLocal = False
        Try
            If p_FromEmail = "" Then
                p_ErrorMessage = "A from address is required"
                Exit Function
            End If
            If p_ToEmail = "" Then
                p_ErrorMessage = "A to address is required"
                Exit Function
            End If

            Dim mm As New System.Net.Mail.MailMessage(p_FromEmail, p_ToEmail)

            '(2) Assign the MailMessage's properties
            mm.Subject = p_Subject
            If p_CCEmail <> "" Then
                mm.CC.Add(p_CCEmail)
            End If
            mm.Body = p_Message
            mm.IsBodyHtml = p_HtmlEncoded
            Dim smtp As New System.Net.Mail.SmtpClient
            Dim basicAuthenticationInfo As New System.Net.NetworkCredential("", "")
            smtp.UseDefaultCredentials = False
            smtp.Credentials = basicAuthenticationInfo
            smtp.Send(mm)
            sendEmailLocal = True
        Catch ex As Exception
            p_ErrorMessage = ex.Message
        End Try

    End Function
End Class
