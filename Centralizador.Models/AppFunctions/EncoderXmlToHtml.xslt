<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>
  <!--<xsl:template match="@* | node()"> : Atributo + comentarios + Texto nodo y elemento. LO MISMO: attribute::* | child::node()
      <xsl:template match="@* | *">      : Atributo y elemento.
      <xsl:template match="/">           : Nodo raíz.-->

  <!--. Nodo actual
     .. Primario del nodo actual
      | Alternativas (nombre|apellido)
     // Hace que coincidan los descendientes en vez de los secundarios
     * Comodín; hace que coincidan todos los elementos
     @ Selección de atributo
     [] Filtro para afinar
     $all$ Todos los nodos deben de cumplir condición
     $any$ Algun nodo debe de cumplir la condición-->
  <xsl:decimal-format name="cl" decimal-separator="," grouping-separator="."/>
  <xsl:param name="timbre"> </xsl:param>
  <xsl:template match="/">
    <html>
      <head>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
        <title>Factura electrónica DTE</title>
        <style type="text/css">
          #datos1 {
          border: 2px solid #000;
          padding:5px;
          min-width:750px;
          position:relative;

          border-radius:6px;
          -moz-border-radius:6px; /* Firefox */
          -webkit-border-radius:6px; /* Safari y Chrome */
          }

          .paddingtopbottom {
          height: 18px;
          }
          .tabladescripcion {
          font-size: 10px;
          border-radius:6px;
          -moz-border-radius:6px; /* Firefox */
          -webkit-border-radius:6px; /* Safari y Chrome */
          border: 2px solid #000;
          }

          .tablatotal {
          border-radius:6px;
          -moz-border-radius:6px; /* Firefox */
          -webkit-border-radius:6px; /* Safari y Chrome */
          border: 2px solid #000;
          float: right;
          }

          .txt1 {
          font-size: 13px;
          color: #000;
          }
          .txt2 {
          font-size: 12px;
          color: #000;
          font-family: Arial, Helvetica, sans-serif;
          }
          .txt3 {
          font-size: 11px;
          color: #000;
          text-align: center;
          }
          .negrita1 {
          font-weight: bold;
          }
          body {
          font-family: Arial, Helvetica, sans-serif;
          }
          .mayuscula1 {
          text-transform: uppercase;
          }

          .borde-der-abajo{
          border-right-width: 2px;
          border-bottom-width: 2px;
          border-right-style: solid;
          border-bottom-style: solid;
          border-right-color: #000;
          border-bottom-color: #000;
          }

          .borde-abajo{
          border-bottom-width: 2px;
          border-bottom-style: solid;
          border-bottom-color: #000;
          }

          .borde-der{
          font-size: 12px;
          color: #000;
          font-family: Arial, Helvetica, sans-serif;
          border-right-width: 2px;
          border-right-style: solid;
          border-right-color: #000;
          padding-right: 8px;
          }
          #cuadro-numfactura {
          height: 120px;
          width: 300px;
          border: 2px solid #ff1107;
          float: right;
          }
          .tit1 {
          font-size: 20px;
          }
          .tit2 {
          font-size: 14px;
          }
          .tit3 {
          font-size: 16px;
          color: #ff1107;
          font-weight: bold;
          text-transform: uppercase;
          }
          .margender {
          margin-right: 5px;
          }
          .borde-puntos {
          border-bottom-width: 1px;
          border-bottom-style: dotted;
          border-bottom-color: #666;
          }
          .justificado {
          text-align: justify;
          }
          .margenizq {
          margin-left: 5px;
          }
        </style>
      </head>
      <form>
        <body>
          <table width="100%" border="0" cellpadding="0">
            <tr>
              <td>
                <!--TABLA DEL HEADER-->
                <table width="100%" border="0" cellpadding="0" cellspacing="0">
                  <tr>
                    <!--MEMBRETE EMISOR-->
                    <td width="38%">
                      <p>
                        <span class="tit1">
                          <xsl:value-of select="Documento/Encabezado/Emisor/RznSoc"/>
                        </span>
                        <br></br>
                        <span class="tit2">
                          <xsl:value-of select="Documento/Encabezado/Emisor/GiroEmis"/>
                        </span>
                      </p>
                      <p>
                        <span class="txt2">
                          <xsl:value-of select="Documento/Encabezado/Emisor/DirOrigen"/>-<xsl:value-of select="Documento/Encabezado/Emisor/CmnaOrigen"/>
                        </span>
                        <br></br>
                        <span class="txt2">
                          <xsl:value-of select="Documento/Encabezado/Emisor/CiudadOrigen"/>
                        </span>
                      </p>
                    </td>
                    <!--COLUMNA VACÍA DEL CENTRO-->
                    <td width="27%">&#160;</td>
                    <!--RECTÁNGULO ROJO EMISOR-->
                    <td width="35%" align="center">
                      <div id="cuadro-numfactura" style="padding-bottom: 8px;">
                        <p>
                          <span class="tit3">
                            R.U.T. :
                            <xsl:call-template name="formatearRut">
                              <xsl:with-param name="input" select="Documento/Encabezado/Emisor/RUTEmisor"/>
                            </xsl:call-template>
                          </span>
                        </p>
                        <p>
                          <span class="tit3">
                            <xsl:call-template name="DTEName">
                              <xsl:with-param name="codDTE" select="Documento/Encabezado/IdDoc/TipoDTE"/>
                            </xsl:call-template>


                          </span>
                        </p>
                        <p>
                          <span class="tit3">
                            N°
                            <xsl:value-of select="Documento/Encabezado/IdDoc/Folio"/>
                          </span>
                        </p>
                      </div>
                      <!--FIN RECTÁNGULO ROJO EMISOR-->
                    </td>
                  </tr>
                  <tr>
                    <td>&#160;</td>
                    <td>&#160;</td>
                    <td align="center">&#160;</td>
                  </tr>
                </table>
              </td>
            </tr>
            <tr>
              <td>
                <!--DATOS DEL RECEPTOR-->
                <div class="bordecurvo" id="datos1">
                  <table width="98%" border="0" align="center" cellpadding="0" cellspacing="0">
                    <tr>
                      <td width="10%">
                        <span class="txt2 negrita1">Señor(es)</span>
                      </td>
                      <td width="45%">
                        <span class="txt2 mayuscula1">
                          :
                          <xsl:value-of select="Documento/Encabezado/Receptor/RznSocRecep"/>
                        </span>
                      </td>
                      <td width="15%">
                        <span class="txt2 negrita1">Fecha de emisión</span>
                      </td>
                      <td width="30%">
                        <span class="txt2 mayuscula1">
                          :
                          <xsl:call-template name="format-fecha-all">
                            <xsl:with-param name="input" select="Documento/Encabezado/IdDoc/FchEmis"/>
                            <xsl:with-param name="nombre" select="'mayuscorto'"/>
                            <xsl:with-param name="separador" select="'-'"/>
                          </xsl:call-template>
                        </span>
                      </td>
                    </tr>
                    <tr>
                      <td>
                        <span class="txt2 negrita1">Giro</span>
                      </td>
                      <td>
                        <span class="txt2 mayuscula1">
                          :
                          <xsl:value-of select="Documento/Encabezado/Receptor/GiroRecep"/>
                        </span>
                      </td>
                      <td width="15%">
                        <span class="txt2 negrita1">Fecha de vencimiento</span>
                      </td>
                      <td width="30%">
                        <span class="txt2 mayuscula1">
                          :
                          <xsl:if test="Documento/Encabezado/IdDoc/FchVenc[.!= '']">
                            <xsl:call-template name="format-fecha-all">
                              <xsl:with-param name="input" select="Documento/Encabezado/IdDoc/FchVenc"/>
                              <xsl:with-param name="nombre" select="'mayuscorto'"/>
                              <xsl:with-param name="separador" select="'-'"/>
                            </xsl:call-template>
                          </xsl:if>
                        </span>
                      </td>
                    </tr>
                    <tr>
                      <td>
                        <span class="txt2 negrita1">RUT</span>
                      </td>
                      <td>
                        <span class="txt2 mayuscula1">
                          :
                          <xsl:call-template name="formatearRut">
                            <xsl:with-param name="input" select="Documento/Encabezado/Receptor/RUTRecep"/>
                          </xsl:call-template>
                        </span>
                      </td>
                      <td width="15%">
                        <span class="txt2 negrita1">Condición de pago</span>
                      </td>
                      <td width="30%">
                        <span class="txt2 mayuscula1">
                          :
                          <xsl:choose>
                            <xsl:when test="number(Documento/Encabezado/IdDoc/FmaPago)=1"> Contado</xsl:when>
                            <xsl:when test="number(Documento/Encabezado/IdDoc/FmaPago)=2"> Crédito</xsl:when>
                            <xsl:when test="number(Documento/Encabezado/IdDoc/FmaPago)=3"> Sin Costo(entrega gratuita)</xsl:when>
                          </xsl:choose>
                        </span>
                      </td>
                    </tr>
                    <tr>
                      <td>
                        <span class="txt2 negrita1">Dirección</span>
                      </td>
                      <td>
                        <span class="txt2 mayuscula1">
                          :
                          <xsl:value-of select="Documento/Encabezado/Receptor/DirRecep"/>
                        </span>
                      </td>
                      <td width="15%">
                        <span class="txt2 negrita1">Vendedor</span>
                      </td>
                      <td width="30%">
                        <span class="txt2 mayuscula1">
                          :
                          <xsl:if test="Documento/Encabezado/Receptor/CdgVendedor!='0'">
                            <xsl:value-of select="Documento/Encabezado/Emisor/CdgVendedor"/>
                          </xsl:if>
                        </span>
                      </td>
                    </tr>
                    <tr>
                      <td>
                        <span class="txt2 negrita1">Comuna</span>
                      </td>
                      <td>
                        <span class="txt2 mayuscula1">
                          :
                          <xsl:value-of select="Documento/Encabezado/Receptor/CmnaRecep"/>
                        </span>
                      </td>
                      <td width="15%">
                        <span class="txt2 negrita1">Código Cliente</span>
                      </td>
                      <td width="30%">
                        <span class="txt2 mayuscula1">
                          :
                          <xsl:if test="Documento/Encabezado/Receptor/CdgIntRecep!='0'">
                            <xsl:value-of select="Documento/Encabezado/Receptor/CdgIntRecep"/>
                          </xsl:if>
                        </span>
                      </td>
                    </tr>
                    <tr>
                      <td>
                        <span class="txt2 negrita1">Ciudad</span>
                      </td>
                      <td>
                        <span class="txt2 mayuscula1">
                          :
                          <xsl:value-of select="Documento/Encabezado/Receptor/CiudadRecep"/>
                        </span>
                      </td>
                      <td width="15%">
                        <span class="txt2 negrita1">Fecha de Generación</span>
                      </td>
                      <td width="30%">
                        <span class="txt2 mayuscula1">
                          :
                          <xsl:call-template name="format-fecha-all">
                            <xsl:with-param name="input" select="Documento/TmstFirma"/>
                            <xsl:with-param name="nombre" select="'mayuscorto'"/>
                            <xsl:with-param name="separador" select="'-'"/>
                          </xsl:call-template>
                        </span>
                      </td>
                    </tr>
                  </table>
                </div>
              </td>
            </tr>
            <tr>
              <td>
                <!--REFERENCIAS-->
                <div class="bordecurvo" id="datos1">
                  <table width="98%" border="0" cellpadding="0" cellspacing="0">
                    <xsl:for-each select="Documento/Referencia[.!='']">
                      <tr>
                        <td width="15%" style="">
                          <span class="txt2 negrita1"> Tipo Documento :</span>
                          <span class="txt3 mayuscula1">
                            <xsl:value-of select="TpoDocRef"/>
                          </span>
                        </td>
                        <td width="20%">
                          <span class="txt2 negrita1"> Folio :</span>
                          <span class="txt3 mayuscula1">
                            <xsl:value-of select="FolioRef"/>
                          </span>
                        </td>
                        <td width="15%">
                          <span class="txt2 negrita1"> Fecha :</span>
                          <span class="txt3 mayuscula1">
                            <xsl:if test="FchRef[.!= '']">
                              <xsl:call-template name="format-fecha-all">
                                <xsl:with-param name="input" select="FchRef"/>
                                <xsl:with-param name="nombre" select="'corto'"/>
                                <xsl:with-param name="separador" select="'-'"/>
                              </xsl:call-template>
                            </xsl:if>
                          </span>
                        </td>
                        <td width="20%">
                          <span class="txt2 negrita1"> Tipo Referencia :</span>
                          <span class="txt3 mayuscula1">
                            <xsl:value-of select="TpoDocRef"/>
                          </span>
                        </td>
                        <td width="30%">
                          <span class="txt2 negrita1"> Razón Referencia :</span>
                          <span class="txt3 mayuscula1">
                            <xsl:value-of select="RazonRef"/>
                          </span>
                        </td>
                      </tr>
                    </xsl:for-each>
                  </table>
                </div>
              </td>
            </tr>
            <tr>
              <td>
                <!--TABLA DE DETALLE-->
                <table width="100%" border="0" cellpadding="0" cellspacing="0" class="tabladescripcion">
                  <tr>
                    <td width="8%" align="center" class="borde-der-abajo  paddingtopbottom">
                      <span class="negrita1 mayuscula1 txt2">cant.</span>
                    </td>
                    <td width="11%" align="center" class="borde-der-abajo  paddingtopbottom">
                      <span class="negrita1 mayuscula1 txt2">código</span>
                    </td>
                    <td width="39%" align="center" class="borde-der-abajo  paddingtopbottom">
                      <span class="negrita1 mayuscula1 txt2">descripción</span>
                    </td>
                    <td width="13%" align="center" class="borde-der-abajo">
                      <span class="negrita1 mayuscula1 txt2">p. unit.</span>
                    </td>
                    <td width="8%" align="center" class="borde-der-abajo">
                      <span class="negrita1 mayuscula1 txt2">descuento</span>
                    </td>
                    <td width="8%" align="center" class="borde-der-abajo">
                      <span class="negrita1 mayuscula1 txt2">recargos</span>
                    </td>
                    <td width="13%" align="center" class="borde-abajo">
                      <span class="negrita1 mayuscula1 txt2">total</span>
                    </td>
                  </tr>
                  <xsl:for-each select="Documento/Detalle[.!='']">
                    <tr>
                      <!--CANTIDAD-->
                      <xsl:choose>
                        <xsl:when test="QtyItem[.!= '']">
                          <td align="center" class="borde-der txt2">
                            <xsl:call-template name="formatea-number">
                              <xsl:with-param name="val" select="QtyItem"/>
                              <xsl:with-param name="format-string" select="'###.###'"/>
                              <xsl:with-param name="locale" select="'cl'"/>
                            </xsl:call-template>
                          </td>
                        </xsl:when>
                        <xsl:otherwise>
                          <td align="center" class="borde-der txt2"></td>
                        </xsl:otherwise>
                      </xsl:choose>
                      <!--CÓDIGO-->
                      <xsl:choose>
                        <xsl:when test="VlrCodigo[.!= '']">
                          <td align="center" class="borde-der">
                            <xsl:call-template name="formatea-number">
                              <xsl:with-param name="val" select="VlrCodigo"/>
                              <xsl:with-param name="format-string" select="'###.###'"/>
                              <xsl:with-param name="locale" select="'cl'"/>
                            </xsl:call-template>
                          </td>
                        </xsl:when>
                        <xsl:otherwise>
                          <td align="center" class="borde-der"></td>
                        </xsl:otherwise>
                      </xsl:choose>
                      <!--DESCRIPCION-->
                      <xsl:variable name="break">&#10;</xsl:variable>
                      <xsl:choose>
                        <xsl:when test="NmbItem[.!= '']">
                          <td align="center" class="borde-der">
                            <xsl:value-of select="NmbItem[.!='']"/>
                            <br></br>
                            <xsl:value-of select="DscItem[.!='']"/>
                          </td>
                        </xsl:when>
                        <xsl:otherwise>
                          <td align="center" class="borde-der"></td>
                        </xsl:otherwise>
                      </xsl:choose>

                      <!--P. UNITARIO-->
                      <xsl:choose>
                        <xsl:when test="PrcItem[.!= '']">
                          <td align="right" class="borde-der">
                            <xsl:call-template name="formatea-number">
                              <xsl:with-param name="val" select="PrcItem"/>
                              <xsl:with-param name="format-string" select="'###.###'"/>
                              <xsl:with-param name="locale" select="'cl'"/>
                            </xsl:call-template>
                          </td>
                        </xsl:when>
                        <xsl:otherwise>
                          <td align="center" class="borde-der"></td>
                        </xsl:otherwise>
                      </xsl:choose>

                      <!--DESCUENTOS-->
                      <td align="center" class="borde-der"></td>
                      <!--RECARGOS-->
                      <td class="borde-der "></td>

                      <!--TOTAL-->
                      <xsl:choose>
                        <xsl:when test="MontoItem[.!= '']">
                          <td align="right" class="borde-der">
                            <xsl:call-template name="formatea-number">
                              <xsl:with-param name="val" select="MontoItem"/>
                              <xsl:with-param name="format-string" select="'###.###'"/>
                              <xsl:with-param name="locale" select="'cl'"/>
                            </xsl:call-template>
                          </td>
                        </xsl:when>
                        <xsl:otherwise>
                          <td align="center" class="borde-der"></td>
                        </xsl:otherwise>
                      </xsl:choose>
                    </tr>
                  </xsl:for-each>
                </table>
                <!--FIN TABLA DETALLE-->
              </td>
            </tr>
            <tr>
              <td>
                <!--espaciado-->
                <br></br>
                <!--fin espaciado-->
                <table width="100%" border="0" cellpadding="0" cellspacing="0">
                  <tr>
                    <td width="40%" align="center" >
                      <img border="0" width="352px" height="152px">
                        <xsl:attribute name="src">
                          <xsl:value-of select="$timbre" />
                        </xsl:attribute>
                      </img>
                      <br>
                        <span class="txt3"/>
                        Timbre Electrónico SII
                      </br>Verifique documento en www.sii.cl
                    </td>
                    <td width="30%">
                      <!--TABLA ACUSE DE RECIBO-->
                      <table border="0" cellspacing="0" cellpadding="0" class="margender">
                        <tr>
                          <td width="25%">
                            <span class="txt1 negrita1">Nombre:</span>
                          </td>
                          <td width="75%" class="borde-puntos">&#160;</td>
                        </tr>
                        <tr>
                          <td>
                            <span class="txt1 negrita1">Rut:</span>
                          </td>
                          <td class="borde-puntos">&#160;</td>
                        </tr>
                        <tr>
                          <td>
                            <span class="txt1 negrita1">Fecha:</span>
                          </td>
                          <td class="borde-puntos">&#160;</td>
                        </tr>
                        <tr>
                          <td>
                            <span class="txt1 negrita1">Recinto:</span>
                          </td>
                          <td class="borde-puntos">&#160;</td>
                        </tr>
                        <tr>
                          <td>
                            <span class="txt1 negrita1">Firma:</span>
                          </td>
                          <td class="borde-puntos">&#160;</td>
                        </tr>
                        <tr>
                          <td>&#160;</td>
                          <td>&#160;</td>
                        </tr>
                        <tr>
                          <td colspan="2" class="justificado">
                            <span class="txt1">El acuse recibo que se declara en este acto, de acuerdo a lo dispuesto en la letra b) del Art. 4°, y la letra c) del Art. 5° de la ley 19.983, acredita que la entrega de mercaderias o servicio(s) prestado(s) han sido recibido(s).</span>
                          </td>
                        </tr>
                      </table>
                      <!--FIN TABLA ACUSE DE RECIBO-->
                    </td>
                    <td width="30%">
                      <table width="100%" border="0" cellpadding="0" cellspacing="0" class="tablatotal">
                        <tr class="paddingtopbottom">
                          <td class="borde-der-abajo">
                            <span class="negrita1 txt1 mayuscula1 margenizq">recargos</span>
                          </td>
                          <td align="right" class="borde-abajo">
                            <span class="negrita1 txt1 mayuscula1 margender">0</span>
                          </td>
                        </tr>
                        <tr class="paddingtopbottom">
                          <td class="borde-der-abajo">
                            <span class="negrita1 txt1 mayuscula1 margenizq">descuentos</span>
                          </td>
                          <td align="right" class="borde-abajo">
                            <span class="negrita1 txt1 mayuscula1 margender">0</span>
                          </td>
                        </tr>
                        <tr class="paddingtopbottom">
                          <td width="50%" class="borde-der-abajo">
                            <span class="negrita1 txt1 mayuscula1 margenizq">monto neto</span>
                          </td>
                          <td width="50%" align="right" class="borde-abajo">
                            <span class="negrita1 txt1 mayuscula1 margender">
                              <!--MONTO NETO-->
                              <xsl:choose>
                                <xsl:when test="Documento/Encabezado/Totales/MntNeto[.!='']">
                                  <xsl:call-template name="formatea-number">
                                    <xsl:with-param name="val" select="Documento/Encabezado/Totales/MntNeto"/>
                                    <xsl:with-param name="format-string" select="'###.###'"/>
                                    <xsl:with-param name="locale" select="'cl'"/>
                                  </xsl:call-template>
                                </xsl:when>
                                <xsl:otherwise>
                                  0
                                </xsl:otherwise>
                              </xsl:choose>
                            </span>
                          </td>
                        </tr>
                        <tr class="paddingtopbottom">
                          <td class="borde-der-abajo">
                            <span class="negrita1 txt1 mayuscula1 margenizq">monto exento</span>
                          </td>
                          <td align="right" class="borde-abajo">
                            <span class="negrita1 txt1 mayuscula1 margender">
                              <!--MONTO EXENTO-->
                              <xsl:choose>
                                <xsl:when test="Documento/Encabezado/Totales/MntExe[.!='0']">
                                  <xsl:call-template name="formatea-number">
                                    <xsl:with-param name="val" select="Documento/Encabezado/Totales/MntExe"/>
                                    <xsl:with-param name="format-string" select="'###.###'"/>
                                    <xsl:with-param name="locale" select="'cl'"/>
                                  </xsl:call-template>
                                </xsl:when>
                                <xsl:otherwise>
                                  0
                                </xsl:otherwise>
                              </xsl:choose>
                            </span>
                          </td>
                        </tr>
                        <tr class="paddingtopbottom">
                          <td class="borde-der-abajo">
                            <span class="negrita1 txt1 mayuscula1 margenizq">19% i.v.a:</span>
                          </td>
                          <td align="right" class="borde-abajo">
                            <span class="negrita1 txt1 mayuscula1 margender">
                              <!--MONTO IVA-->
                              <!--DEVUELVE CERO SI NO HAY DATO-->
                              <xsl:choose>
                                <xsl:when test="Documento/Encabezado/Totales/IVA[.!='']">
                                  <xsl:call-template name="formatea-number">
                                    <xsl:with-param name="val" select="Documento/Encabezado/Totales/IVA"/>
                                    <xsl:with-param name="format-string" select="'###.###'"/>
                                    <xsl:with-param name="locale" select="'cl'"/>
                                  </xsl:call-template>
                                </xsl:when>
                                <xsl:otherwise>
                                  0
                                </xsl:otherwise>
                              </xsl:choose>
                            </span>
                          </td>
                        </tr>
                        <tr class="paddingtopbottom">
                          <td class="borde-der-abajo">
                            <span class="negrita1 txt1 mayuscula1 margenizq">Imp. y/o Ret.</span>
                          </td>
                          <td align="right" class="borde-abajo">
                            <span class="negrita1 txt1 mayuscula1 margender">0</span>
                          </td>
                        </tr>
                        <tr class="paddingtopbottom">
                          <td class="borde-der-abajo">
                            <span class="negrita1 txt1 mayuscula1 margenizq">monto total</span>
                          </td>
                          <td align="right" class="borde-abajo">
                            <span class="negrita1 txt1 mayuscula1 margender">
                              <!--MONTO TOTAL-->
                              <xsl:choose>
                                <xsl:when test="Documento/Encabezado/Totales/MntTotal[.!='']">
                                  <xsl:call-template name="formatea-number">
                                    <xsl:with-param name="val" select="Documento/Encabezado/Totales/MntTotal"/>
                                    <xsl:with-param name="format-string" select="'###.###'"/>
                                    <xsl:with-param name="locale" select="'cl'"/>
                                  </xsl:call-template>
                                </xsl:when>
                                <xsl:otherwise>
                                  0
                                </xsl:otherwise>
                              </xsl:choose>
                            </span>
                          </td>
                        </tr>
                        <tr class="paddingtopbottom">
                          <td class="borde-der-abajo">
                            <span class="negrita1 txt1 mayuscula1 margenizq">monto n/ fact.</span>
                          </td>
                          <td align="right" class="borde-abajo">
                            <span class="negrita1 txt1 mayuscula1 margender">0</span>
                          </td>
                        </tr>
                        <tr class="paddingtopbottom">
                          <td class="borde-der-abajo">
                            <span class="negrita1 txt1 mayuscula1 margenizq">monto periodo</span>
                          </td>
                          <td align="right" class="borde-abajo">
                            <span class="negrita1 txt1 mayuscula1 margender">0</span>
                          </td>
                        </tr>
                        <tr class="paddingtopbottom">
                          <td class="borde-der-abajo">
                            <span class="negrita1 txt1 mayuscula1 margenizq">saldo anterior</span>
                          </td>
                          <td align="right" class="borde-abajo">
                            <span class="negrita1 txt1 mayuscula1 margender">0</span>
                          </td>
                        </tr>
                        <tr class="paddingtopbottom">
                          <td class="borde-der">
                            <span class="negrita1 txt1 mayuscula1 margenizq">valor a pagar</span>
                          </td>
                          <td align="right">
                            <span class="negrita1 txt1 mayuscula1 margender">0</span>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
            <tr>
              <td align="right">
              </td>
            </tr>
          </table>
        </body>
      </form>
    </html>
  </xsl:template>
  <xsl:template name="formatearRut">
    <!-- Funcion para Formatear Rut   -->
    <xsl:param name="input"/>
    <xsl:variable name="rut" select="substring-before($input, '-')"/>
    <xsl:variable name="last" select="substring($rut,string-length($rut)-2,3)"/>
    <xsl:variable name="middle" select="substring($rut,string-length($rut)-5,3)"/>
    <xsl:variable name="first">
      <xsl:choose>
        <xsl:when test="string-length($rut)=7">
          <xsl:value-of select="substring($rut,1,1)"/>
        </xsl:when>
        <xsl:when test="string-length($rut)=8">
          <xsl:value-of select="substring($rut,1,2)"/>
        </xsl:when>
        <xsl:when test="string-length($rut)=9">
          <xsl:value-of select="substring($rut,1,3)"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="dv" select="substring-after($input, '-')"/>
    <xsl:value-of select="concat($first,'.',$middle,'.',$last, '-', $dv)"/>
  </xsl:template>

  <xsl:template name="DTEName">
    <xsl:param name="codDTE"/>
    <xsl:variable name="codDTEnum" select="number($codDTE)"/>
    <xsl:choose>
      <xsl:when test="$codDTEnum = 33">FACTURA ELECTRÓNICA</xsl:when>
      <xsl:when test="$codDTEnum = 34">FACTURA NO AFECTA O EXENTA ELECTRÓNICA</xsl:when>
      <xsl:when test="$codDTEnum = 43">LIQUIDACIÓN DE FACTURA</xsl:when>
      <xsl:when test="$codDTEnum = 46">FACTURA DE COMPRA ELECTRÓNICA</xsl:when>
      <xsl:when test="$codDTEnum = 52">GUIA DE DESPACHO ELECTRÓNICA</xsl:when>
      <xsl:when test="$codDTEnum = 56">NOTA DE DEBITO ELECTRÓNICA</xsl:when>
      <xsl:when test="$codDTEnum = 61">NOTA DE CREDITO ELECTRÓNICA</xsl:when>
      <xsl:when test="$codDTEnum = 39">BOLETA ELECTRÓNICA</xsl:when>
      <xsl:when test="$codDTEnum = 110">FACTURA DE EXPORTACIÓN ELECTRÓNICA</xsl:when>
      <xsl:when test="$codDTEnum = 111">NOTA DE DEBITO DE EXPORTACIÓN ELECTRÓNICA</xsl:when>
      <xsl:when test="$codDTEnum = 112">NOTA DE CREDITO DE EXPORTACIÓN ELECTRÓNICA</xsl:when>
      <xsl:when test="$codDTE = 'SET'">SET</xsl:when>
      <xsl:otherwise>
        DESCONOCIDO(
        <xsl:value-of select="concat($codDTE, '-', $codDTEnum)"/>
        )
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="format-fecha-all">
    <!--
  Recibe fecha formato 2005-02-12 devuelve Fecha formateada dd-mm-aaaa u otros formatos segun parametros
        nombre = nombre
        nombre = corto
        nombre = nombremayus
        nombre = mayuscorto
        separador = - / de 
-->
    <xsl:param name="input" select="''"/>
    <xsl:param name="nombre" select="''"/>
    <xsl:param name="separador" select="'-'"/>
    <xsl:variable name="year" select="substring($input, 1, 4)"/>
    <xsl:variable name="mes">
      <xsl:call-template name="dos-digitos">
        <xsl:with-param name="mes-id" select="substring($input, 6, 2)"/>
        <xsl:with-param name="id" select="'mes'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="day">
      <xsl:call-template name="dos-digitos">
        <xsl:with-param name="mes-id" select="substring($input, 9, 2)"/>
        <xsl:with-param name="id" select="'dia'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$nombre = 'nombre'">
        <xsl:variable name="nom">
          <xsl:call-template name="nombre-mes">
            <xsl:with-param name="tip" select="$mes"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:value-of select="concat($day, $separador, $nom, $separador, $year)"/>
      </xsl:when>
      <xsl:when test="$nombre = 'corto'">
        <xsl:variable name="nomb">
          <xsl:call-template name="nom-mes">
            <xsl:with-param name="tip" select="$mes"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:value-of select="concat($day, $separador, $nomb, $separador, $year)"/>
      </xsl:when>
      <xsl:when test="$nombre = 'nombremayus'">
        <xsl:variable name="nomM">
          <xsl:call-template name="nombre-mes-mayus">
            <xsl:with-param name="tip" select="$mes"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:value-of select="concat($day, $separador, $nomM, $separador, $year)"/>
      </xsl:when>
      <xsl:when test="$nombre = 'mayuscorto'">
        <xsl:variable name="nomMC">
          <xsl:call-template name="nom-mes-mayus">
            <xsl:with-param name="tip" select="$mes"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:value-of select="concat($day, $separador, $nomMC, $separador, $year)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat($day, $separador, $mes, $separador, $year)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="dos-digitos">
    <!--
 Recibe digitos y lo devuelve con dos digitos 
        si por ej: viene 1 devuelve 01  mes-id recibe el valor, id recibe si se formatea mes o dia 
-->
    <xsl:param name="mes-id" select="0"/>
    <xsl:param name="id" select="'mes'"/>
    <xsl:choose>
      <xsl:when test="$id ='mes'">
        <!--  Para mes  -->
        <xsl:choose>
          <xsl:when test="number($mes-id) >= 1 and number($mes-id) &lt;= 12">
            <xsl:value-of select="format-number(number($mes-id), '00')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="ERR"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <!--  Para dia  -->
        <xsl:choose>
          <xsl:when test="$id = 'dia'">
            <xsl:value-of select="format-number(number($mes-id), '00')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="ERR"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="nom-mes-mayus">
    <!--  Esta funcion recibe 02 y devuelve FEB  -->
    <xsl:param name="tip"/>
    <xsl:variable name="resultado">
      <xsl:choose>
        <xsl:when test="$tip='01'">ENE</xsl:when>
        <xsl:when test="$tip='02'">FEB</xsl:when>
        <xsl:when test="$tip='03'">MAR</xsl:when>
        <xsl:when test="$tip='04'">ABR</xsl:when>
        <xsl:when test="$tip='05'">MAY</xsl:when>
        <xsl:when test="$tip='06'">JUN</xsl:when>
        <xsl:when test="$tip='07'">JUL</xsl:when>
        <xsl:when test="$tip='08'">AGO</xsl:when>
        <xsl:when test="$tip='09'">SEP</xsl:when>
        <xsl:when test="$tip='10'">OCT</xsl:when>
        <xsl:when test="$tip='11'">NOV</xsl:when>
        <xsl:when test="$tip='12'">DIC</xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$resultado">
        <xsl:value-of select="$resultado"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$tip"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="nombre-mes">
    <!--
 Esta funcion recibe un n&#250;mero y devuelve el nombre del mes que
          le corresponde Se utiliza para pcl principalmente.
       
-->
    <xsl:param name="tip"/>
    <xsl:variable name="resultado">
      <xsl:choose>
        <xsl:when test="$tip='01'">Enero</xsl:when>
        <xsl:when test="$tip='02'">Febrero</xsl:when>
        <xsl:when test="$tip='03'">Marzo</xsl:when>
        <xsl:when test="$tip='04'">Abril</xsl:when>
        <xsl:when test="$tip='05'">Mayo</xsl:when>
        <xsl:when test="$tip='06'">Junio</xsl:when>
        <xsl:when test="$tip='07'">Julio</xsl:when>
        <xsl:when test="$tip='08'">Agosto</xsl:when>
        <xsl:when test="$tip='09'">Septiembre</xsl:when>
        <xsl:when test="$tip='10'">Octubre</xsl:when>
        <xsl:when test="$tip='11'">Noviembre</xsl:when>
        <xsl:when test="$tip='12'">Diciembre</xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$resultado">
        <xsl:value-of select="$resultado"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$tip"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="nom-mes">
    <!--  Esta funcion recibe 02 y devuelve Feb  -->
    <xsl:param name="tip"/>
    <xsl:variable name="resultado">
      <xsl:choose>
        <xsl:when test="$tip='01'">Ene</xsl:when>
        <xsl:when test="$tip='02'">Feb</xsl:when>
        <xsl:when test="$tip='03'">Mar</xsl:when>
        <xsl:when test="$tip='04'">Abr</xsl:when>
        <xsl:when test="$tip='05'">May</xsl:when>
        <xsl:when test="$tip='06'">Jun</xsl:when>
        <xsl:when test="$tip='07'">Jul</xsl:when>
        <xsl:when test="$tip='08'">Ago</xsl:when>
        <xsl:when test="$tip='09'">Sep</xsl:when>
        <xsl:when test="$tip='10'">Oct</xsl:when>
        <xsl:when test="$tip='11'">Nov</xsl:when>
        <xsl:when test="$tip='12'">Dic</xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$resultado">
        <xsl:value-of select="$resultado"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$tip"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="nombre-mes-mayus">
    <!--
 Esta funcion recibe un n&#250;mero y devuelve el nombre del mes que
          le corresponde Se utiliza para pcl principalmente.     
-->
    <xsl:param name="tip"/>
    <xsl:variable name="resultado">
      <xsl:choose>
        <xsl:when test="$tip='01'">ENERO</xsl:when>
        <xsl:when test="$tip='02'">FEBRERO</xsl:when>
        <xsl:when test="$tip='03'">MARZO</xsl:when>
        <xsl:when test="$tip='04'">ABRIL</xsl:when>
        <xsl:when test="$tip='05'">MAYO</xsl:when>
        <xsl:when test="$tip='06'">JUNIO</xsl:when>
        <xsl:when test="$tip='07'">JULIO</xsl:when>
        <xsl:when test="$tip='08'">AGOSTO</xsl:when>
        <xsl:when test="$tip='09'">SEPTIEMBRE</xsl:when>
        <xsl:when test="$tip='10'">OCTUBRE</xsl:when>
        <xsl:when test="$tip='11'">NOVIEMBRE</xsl:when>
        <xsl:when test="$tip='12'">DICIEMBRE</xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$resultado">
        <xsl:value-of select="$resultado"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$tip"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="divide_en_lineas">
    <!--   template para separar en l&#237;neas   -->
    <xsl:param name="val"/>
    <xsl:param name="c1"/>
    <xsl:choose>
      <xsl:when test="not(contains($val, $c1))">
        <xsl:value-of select="$val"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="string-length(substring-before($val, $c1))=0">
            <xsl:call-template name="divide_en_lineas">
              <xsl:with-param name="val">
                <xsl:value-of select="substring-after($val, $c1)"/>
              </xsl:with-param>
              <xsl:with-param name="c1">
                <xsl:value-of select="$c1"/>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="substring-before($val, $c1)"/>
            <br/>
            <xsl:call-template name="divide_en_lineas">
              <xsl:with-param name="val">
                <xsl:value-of select="substring-after($val, $c1)"/>
              </xsl:with-param>
              <xsl:with-param name="c1">
                <xsl:value-of select="$c1"/>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="formatea-number">
    <!--  template para formatear numeros   -->
    <xsl:param name="val" />
    <xsl:param name="format-string"/>
    <xsl:param name="locale" />
    <xsl:variable name="result" select="format-number($val, $format-string, $locale)"/>
    <xsl:choose>
      <xsl:when test="$result = 'NaN'">
        <xsl:value-of select="0"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$result"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
