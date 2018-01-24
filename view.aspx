<%@ page title="Unviewed Proofs" language="vb" autoeventwireup="false" masterpagefile="~/masterpage.master" codefile="view.aspx.vb" inherits="proof_list" %>

<asp:content contentplaceholderid="scripts" runat="server">
    <script src="assets/js/holmes.js"></script>
    <script>
        $(document).ready(function () {
            $('.button.approve').click(function () {
                var sonum = $(this).parent().parent().find('h4 span').text().replace('<br>', ', ');
                var confirmation = confirm('Approve proof for ' + sonum + '?');
                if (confirmation == false) return false
            });
            $('.button.email').click(function () {
                var sonum = $(this).parent().parent().find('h4 span').text().replace('<br>', ', ');
                var confirmation = confirm('Re-send proof email for ' + sonum + '?');
                if (confirmation == false) return false
            });
            $('.button.delete').click(function () {
                var sonum = $(this).parent().find('h4 span').text().replace('<br>', ', ');
                var confirmation = confirm('Remove proof entry for ' + sonum + '?');
                if (confirmation == false) return false
            });
        });
        var h = new holmes({
            input: '.search input',
            find: '#proofFlex .proofEntry',
            class: {
                visible: 'visible',
                hidden: 'hidden'
            }
        });
    </script>
</asp:content>

<asp:content contentplaceholderid="styles" runat="server">
    <style type="text/css">
        #proofFlex {display:-webkit-flex; display:flex; align-items: flex-start; align-content: flex-start; justify-content: space-around; flex-flow:row wrap;}
        .proofEntry {background:#ddd; padding:1em; border-radius:.3em; margin:1em 0; width:32%; position:relative;}
        .proofEntry h4 {margin-top:0}
        .proofEntry h4 span {float:right; font-size:.9em;}
        .proofEntry .button {float:right; margin-left:.5em; padding:.5em}
        #filters .button {padding:.5em .75em; float:right; margin:0 1em 0 0;}
        #filters h1 {display:inline-block; margin:0;}
        .proofEntry .views {width: calc(100% - 16em);}
        .proofEntry .views a {
            display:inline-block; 
            vertical-align:bottom; 
            padding:.25em; 
            margin:.5em .5em 0 0; 
            text-decoration: none !important; 
            background:rgba(0,0,0,.1); 
            border-radius:.25em;
            color:black;
        }
        .proofEntry .button.delete {padding:0em .3em; border-radius:100%; font-size:1.5em; position:absolute; top:-.5em; right:-.5em; margin:0}
        .proofEntry .buttons {margin-top:1em;}
        
        /* holmes stuff */
        .hidden {display:none;}
        .search {font-size:1.4em; margin:1em; border:1px solid rgba(0,0,0,.5); border-radius:.3em;}
        .search input {border:0; font:normal 1.4em/1em sans-serif;  display:inline-block; width:calc(100% - 1.5em); padding:0; height:auto !important;}

        @media all and (max-width:1070px) {
            .proofEntry {width:48%}
            .proofEntry .views {display:block; clear:both; width:auto;}
            .buttons {display:flex;}
            .buttons a {width:48%;}
        }
        @media all and (max-width:770px) {
            #filters .button {
                clear: both;
                margin-bottom: .5em;
            }
        }
        @media all and (max-width:600px) {
            .proofEntry {width:100%;}
            .proofEntry .button.delete {right:0; top:-.7em;}
            #filters .button, .proofEntry .button {float:none; display:block;}
            #filters {display:block; float:none;}
        }

    </style>
</asp:content>

<asp:content contentplaceholderid="pageContent" runat="server">

    <div id="report" runat="server" visible="false"></div>

    <div id="filters" class="colrow">
        <h1>Unapproved Proofs</h1>
        <a href="view.aspx?account=warwick" class="button" data-icon="">Display Warwick Proofs</a> &nbsp; &nbsp;
        <a href="view.aspx?account=others" class="button" data-icon="" style="background-color:#b200ff">Display 'Other' Proofs</a>
    </div>

    <label class="search colrow" data-icon="">
        <input type="search" placeholder="search unapproved proofs..."/>
    </label>

    <div id="proofFlex">
    <asp:repeater id="proofsList" runat="server" clientidmode="predictable">
        <itemtemplate>
            <div class="proofEntry">
                <%--<div id="report" runat="server" class="report" visible="false"></div>--%>
                <asp:linkbutton id="delete" class="button delete" onclick="delApprove" runat="server" style="background-color:firebrick">×</asp:linkbutton>
                <h4 id="title" runat="server"></h4>
                <span data-icon="" id="dateCreated" runat="server"></span><br />
                <span data-icon="" id="dateViewed" runat="server"></span>
                <div id="mailto" runat="server" data-icon=""></div>
                <div id="comments" runat="server" data-icon="" visible="false"></div>
                <div class="buttons">
                    <asp:linkbutton id="approve" class="button approve" onclick="sendApproval" runat="server" data-icon="" style="background-color:seagreen">Approve</asp:linkbutton>
                    <asp:linkbutton id="resendEmail" class="button email" onclick="sendmail" runat="server" data-icon="">Resend</asp:linkbutton>
                    <%--<a id="viewLink" class="button" runat="server" data-icon="" style="background-color:#d72">View</a>--%>
                </div>
                <div id="viewLinks" class="views" runat="server"></div>
                <asp:hiddenfield id="proofSqlId" runat="server" visible="true" />
                <asp:hiddenfield id="proofPo" runat="server" visible="true" />
                <asp:hiddenfield id="proofSo" runat="server" visible="true" />
                <asp:hiddenfield id="proofid" runat="server" visible="true" />
            </div>
        </itemtemplate>
    </asp:repeater>
    </div>
</asp:content>
