<%@ page title="Proof Creator" language="vb" autoeventwireup="false" masterpagefile="~/masterpage.master" codefile="default.aspx.vb" inherits="proof_default" %>

<asp:content contentplaceholderid="scripts" runat="server">
    <script>
        $(document).ready(function () {

            // lets validate the page without refreshing
            $('#saveProof').click(function () {
                $('div.error, label.error').removeClass("error");
                var formCompleted = true;

                if ($('#account_').val() == "") {
                    formCompleted = false;
                    $('#accountBox').addClass('error');
                }

                if ($('#salesorder_').val() == "") {
                    formCompleted = false;
                    $('#salesorderBox').addClass('error');
                }

                //if ($('#leadtime_').val() == "") {
                //    formCompleted = false;
                //    $('#leadTimeBox').addClass('error');
                //}

                if ($('#mailto_').val() == "") {
                    formCompleted = false;
                    $('#mailtoBox').addClass('error');
                }

                if ($('input[type="file"]').val() == "") {
                    formCompleted = false;
                    $('#proofBox').addClass('error');
                }

                // comments_
                if (!formCompleted) {
                    return false;
                } else {
                    $('body').toggleClass('saving');
                }
            });

            // check blind proof by default for warwick
            $('#account_').change(function () {
                var acctVal = $('#account_').val();
                if (acctVal == "Warwick Publishing") {
                    $('#blindProof').prop("checked", true);
                } else {
                    $('#blindProof').prop("checked", false);
                }
            });

            // add more downloads
            $('.upload:lt(7)').show();
            $('#clone').click(function () {
                $('.upload:hidden:first').slideDown(100);
            });
        });
    </script>
</asp:content>

<asp:content contentplaceholderid="styles" runat="server">
    <style type="text/css">
        #blindProof {
            margin-left: 1em;
        }

        input[type="text"], input[type="password"], input[type="email"], textarea,
        input[type="tel"], input[type="search"], input[type="number"], select,
        input[type="file"] {
            width: 100%;
            height: 3em;
        }

        textarea {
            height: 10.3em;
        }

        input[type="file"] {
            height: 2em;
            padding: .5em;
        }

        .upload {
            margin-bottom: 1em;
            background: rgba(0,0,0,.1);
            padding: .7em;
            border-radius: .5em;
        }

        .upload label.col6 {
            margin-bottom:0
        }

        .upload input {
            padding:0;
            border:1px solid rgba(0,0,0,.2);
            border-radius:.4em;
        }

        .upload input[type="file"]:hover {
            box-shadow: 0 0 .7em red;
        }
        .backtop {display:none;}

        @media all and (max-width:768px) {
            input[type="text"], input[type="password"], input[type="email"], textarea,
            input[type="tel"], input[type="search"], input[type="number"], select,
            input[type="file"] {
                width: 100%;
            }
            .backtop {display:block;}
            #blindProof {
                float: right;
            }
        }
    </style>
</asp:content>

<asp:content contentplaceholderid="pageContent" runat="server">

    <div id="report" runat="server" visible="false"></div>

    <div class="colrow">
        <div class="col6">
            <div id="proofBox" runat="server">
                <div><b>Upload Proof Image</b></div>
                <asp:repeater id="uploadList" runat="server" clientidmode="predictable">
                    <itemtemplate>
                        <div class="upload colrow" style="display: none">
                            <label class="col6">
                                <div>Proof PDF</div>
                                <asp:fileupload id="pdfUpload" runat="server" />
                            </label>
                            <label class="col6">
                                <div>Proof ZAE <small>(optional)</small></div>
                                <asp:fileupload id="zaeUpload" runat="server" />
                            </label>
                        </div>
                    </itemtemplate>
                </asp:repeater>
                <%--<div class="upload">
                    <asp:fileupload id="proofFile" runat="server" />
                </div>--%>
                <a href="#" id="clone" data-icon="" onclick="javascript:return false;">Add another upload</a>
            </div>
        </div>

        <div class="col6 last">
            <label id="accountBox" runat="server">
                <div>Choose Account</div>
                <asp:dropdownlist id="account_" runat="server" required>
                    <asp:listitem value="">Select account...</asp:listitem>
                    <asp:listitem value="Warwick Publishing">Warwick Publishing</asp:listitem>
                    <asp:listitem value="Studio Style">Studio Style</asp:listitem>
                    <asp:listitem value="On The Ball Promotions">On The Ball Promotions</asp:listitem>
                    <asp:listitem value="Easy Pocket Folders">Easy Pocket Folders</asp:listitem>
                </asp:dropdownlist>
                <b>You must choose an account for customers to know which company their order is coming from.</b>
            </label>

            <label>
                <div>Company Identities</div>
                Send blind proof
                <asp:checkbox id="blindProof" runat="server" />
            </label>

            <label id="salesorderBox" runat="server">
                <div>SO Number</div>
                <asp:textbox ID="salesorder_" runat="server" type="number" pattern="[0-9]*" />
                <b>Please enter the appropriate sales order number.</b>
            </label>

            <label id="purchaseorderBox" runat="server">
                <div>PO Number <small>(optional)</small></div>
                <asp:textbox ID="purchaseorder_" runat="server" type="text" />
            </label>

            <label id="leadTimeBox" runat="server">
                <div>Lead Time (number of days)  <small>(optional)</small></div>
                <asp:textbox ID="leadtime_" runat="server" type="number" pattern="[0-9]*" />
            </label>

            <label id="mailtoBox" runat="server">
                <div>Email recipient(s)</div>
                <asp:textbox ID="mailto_" runat="server" placeholder="email@address.com, otheremail@address.com" />
                <b>Please enter any email addresses in which to send the proof.</b>
            </label>

            <label>
                <div>Proof Comments and Info <small>(optional)</small></div>
                <asp:textbox ID="comments_" runat="server" TextMode="multiline" />
            </label>

            <asp:linkbutton runat="server" ID="saveProof" class="button" data-icon-right="">Save and Send Proof</asp:linkbutton>
        </div>
    </div>
</asp:content>
