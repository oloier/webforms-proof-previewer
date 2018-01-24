<%@ page language="vb" autoeventwireup="false" masterpagefile="~/masterpage.master" clientidmode="static" codefile="proof.aspx.vb" inherits="proof_view" %>

<asp:content contentplaceholderid="scripts" runat="server">
        <script src="assets/js/three.min.js"></script>
        <script>
            var model = '<%= modelPath %>';
            <% If modelPath <> "" Then
                Response.WriteFile("assets/js/modelViewer.js")
            End If
            %>
            if (!Detector.webgl) {
                Detector.addGetWebGLMessage();
            } else {
                var dae;
                var loader = new THREE.ColladaLoader();
                loader.options.convertUpAxis = true;
                loader.load(model, function (collada) {
                    dae = collada.scene;
                    dae.scale.x = dae.scale.y = dae.scale.z = 0.002;
                    dae.updateMatrix();
                    init();
                    animate();
                });
            }
        </script>
</asp:content>

<asp:content contentplaceholderid="styles" runat="server">
    <style type="text/css">
        #main {margin-left:0;}
        #side {display:none;}
        .text-center {text-align:center;}
        .text-right {text-align: right;}
        fieldset {border-radius:1em; border:1px solid #aaa;}
        form label input {vertical-align:middle;}
        .left-col {padding:1em;}
        #proofDownload {background-color:#f3ab29;}
        #gotoModel {}
        #proofView {max-width: 100%; margin-bottom:1em;}
        #thumbnails {
            margin:1em 0;
            display: -webkit-box; display: -moz-box; display: -ms-flexbox; display: flex;
            -webkit-flex-flow: row wrap;
            justify-content: flex-start;
        }
        #thumbnails a {
            margin:.5%; 
            border-radius:.5em; 
            border:1px solid rgba(0,0,0,.3);
            width:24%; max-width:49%;
            height:auto; max-height:30%;
            overflow:hidden;
        }
        #thumbnails a:only-child {width:auto; height:auto;}
        #thumbnails img {max-height:100%; max-width:100%;}
        #model canvas {background: #eee; border-radius:1em; max-width:100%; }
        @media all and (min-width:768px) {
            #gotoModel {float:right;}
        }
        @media all and (max-width:768px) {
        }
        @media all and (max-width:600px) {
            #moby {display:none;}
        }
    </style>
</asp:content>

<asp:content contentplaceholderid="pageContent" runat="server">

    <div id="report" runat="server" visible="false"></div>
    
    <section class="colrow" id="pageSection" runat="server">
        <div class="" id="formy" runat="server">
            <h3 style="text-align:left;">Proof for order <asp:label id="proofNumber" runat="server" /></h3>
            <div class="colrow">
                <a runat="server" id="proofDownload" class="button big" data-icon="">Download as PDF</a>
                <a runat="server" id="gotoModel" class="button" data-icon="" visible="false" href="#model">View 3D Virtual Proof</a>
            </div>

            <%--<a runat="server" id="proofZoom" rel="lightbox"><img runat="server" visible="false" id="proofView" /></a>--%>
            <div id="thumbnails" runat="server" visible="false">
                <asp:repeater id="thumbs" runat="server" clientidmode="predictable">
                    <itemtemplate>
                        <a id="thumbLink" runat="server" rel="lightbox">
                            <img id="thumbImg" runat="server" src="#"/>
                        </a>
                    </itemtemplate>
                </asp:repeater>
            </div>
        </div>
        <div id="model"></div>
        <%--<div class="col4 well last">
            <h3>Approve / Request Revisions</h3>
            
            <h4>Please note: </h4>
                <ul>
                    <li>One color imprints are shown in black &amp; white for size and placement only.</li>
                    <li>Color imprints are shown with product outlines, which will not be printed.</li>
                    <li>Because of the wide variety in computer monitor settings and resolutions, colors shown are not 100% accurate.</li>
                </ul>
            <div runat="server" visible="false" id="esBox">
                <strong>Proof Comments:</strong><br />
                <asp:literal id="comments" runat="server" />
            </div>
            <p id="artwork">If new or revised art is required for your proof, please email your file(s) to <a href="mailto:artwork<%=siteDomain %>">artwork@<%=siteDomain %></a></p>
            <!--<p id="dateRow">Your order will be ready to ship on or before <asp:literal id="estShipDate" runat="server" />.</p>-->
        </div>--%>

        <asp:hiddenfield runat="server" id="hiddenpath" />
    </section>
</asp:content>