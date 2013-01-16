<?xml version="1.0" encoding="UTF-8"?>
<!--
///   
/// Copyright 2011 Capgemini & SDL
///
///   Licensed under the Apache License, Version 2.0 (the "License");
///   you may not use this file except in compliance with the License.
///   You may obtain a copy of the License at
///
///       http://www.apache.org/licenses/LICENSE-2.0
///
///   Unless required by applicable law or agreed to in writing, software
///   distributed under the License is distributed on an "AS IS" BASIS,
///   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
///   See the License for the specific language governing permissions and
///   limitations under the License.
-->
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:xhtml="http://www.w3.org/1999/xhtml"
	xmlns:xlink="http://www.w3.org/1999/xlink"
	>
    <xsl:output omit-xml-declaration="yes"/>
	<xsl:template match="@xlink:*">
		<xsl:attribute name="{local-name(.)}"><xsl:value-of select="."/></xsl:attribute>
	</xsl:template>
	<xsl:template match="@xhtml:*">
		<xsl:attribute name="{local-name(.)}"><xsl:value-of select="."/></xsl:attribute>
	</xsl:template>
	<xsl:template match="xhtml:*">
		<xsl:element name="{local-name(.)}">
			<xsl:for-each select="@*">
				<xsl:attribute name="{local-name(.)}"><xsl:value-of select="."/></xsl:attribute>
			</xsl:for-each>
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	</xsl:template>
</xsl:stylesheet>