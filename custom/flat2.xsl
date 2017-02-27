<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes" />
  <xsl:template match="Assets">
    <xsl:copy>
      <xsl:apply-templates select="Asset" />
    </xsl:copy>
  </xsl:template>
  <xsl:template match="Asset|@*">
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <xsl:apply-templates select="Attribute[@name='Description']" mode="blankTemplate" />
      <xsl:apply-templates select="Attribute[@name!='Description']" mode="defaultTemplate" />
    </xsl:copy>
  </xsl:template>
  <xsl:template match="*" mode="defaultTemplate">
	<xsl:element name="{@name}">
		<xsl:value-of select="."/>
	</xsl:element>
  </xsl:template>
  <xsl:template match="*" mode="blankTemplate"/>
</xsl:stylesheet>
