<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output version="1.0" encoding="UTF-8" indent="no" omit-xml-declaration="yes"/>
<xsl:preserve-space elements="*"/>
<xsl:template match="/">
  ## <a name="top">Here's what happened</a>
  Looking at Assembly **<xsl:value-of select="Assembly/Name"/>** of **<xsl:value-of select="Assembly/Time"/>** with the following Scenarios.  
  
  <xsl:variable name="cntScenario" select="count(Assembly/Scenario)" />
  
  <xsl:value-of select="$cntScenario" /> Scenario(s):
  <xsl:if test="$cntScenario = 0">
    [There are no scenarios.]
  </xsl:if>
		
  <xsl:if test="$cntScenario > 0">
    <xsl:for-each select="Assembly/Scenario">
      <xsl:variable name="navID" select="generate-id(Name)" />
  * [<xsl:value-of select="Name" />](&#35;<xsl:value-of select="$navID" />)
    </xsl:for-each>
  </xsl:if>

  
  <xsl:for-each select="Assembly/Scenario">

  ### <a name="{generate-id(Name)}">Scenario</a>
  Name: <xsl:value-of select="Name"/>

  #### Given
<xsl:variable name="cntGivens" select="count(Given)" />
<xsl:choose>
<xsl:when test="$cntGivens = 0">
    [There are no Givens]
</xsl:when>
<xsl:otherwise>
<xsl:for-each select="Given">
<xsl:text>    </xsl:text><xsl:value-of select="Title"/><!--Title-->
<xsl:text>
</xsl:text>
<xsl:if test="Detail != ''" >
<xsl:text>    </xsl:text>
<xsl:for-each select="Detail/Message"><!--Message-->
<xsl:value-of select="concat(., ' ')"/>
</xsl:for-each >
<xsl:text>
</xsl:text>
</xsl:if>
<xsl:if test="position() != last()">
<xsl:text>    </xsl:text>[and]
</xsl:if>
</xsl:for-each >				
</xsl:otherwise>
</xsl:choose>				
				
  #### When
<xsl:variable name="cntWhen" select="count(When)" />
<xsl:choose>
<xsl:when test="$cntWhen = 0">
  [There is no When] 
</xsl:when>
<xsl:otherwise>
<xsl:for-each select="When">
<xsl:text>    </xsl:text><xsl:value-of select="Title"/><!--Title-->
<xsl:text>
</xsl:text>
<xsl:if test="Detail != ''" >
<xsl:text>    </xsl:text>
<xsl:for-each select="Detail/Message"><!--Message-->
<xsl:value-of select="concat(., ' ')"/>
</xsl:for-each >
<xsl:text>
</xsl:text>
</xsl:if>
<xsl:if test="position() != last()">
<xsl:text>    </xsl:text>[and]
</xsl:if>
</xsl:for-each >				
</xsl:otherwise>
</xsl:choose>				

  #### Then
<xsl:variable name="cntThen" select="count(Then)" />
<xsl:choose>
<xsl:when test="$cntThen = 0">
  [There are no Whens]
</xsl:when>
<xsl:otherwise>
<xsl:for-each select="Then">
<xsl:text>    </xsl:text><xsl:value-of select="Title"/><!--Title-->
<xsl:text>
</xsl:text>
<xsl:if test="Detail != ''" >
<xsl:text>    </xsl:text>
<xsl:for-each select="Detail/Message"><!--Message-->
<xsl:value-of select="concat(., ' ')"/>
</xsl:for-each >
<xsl:text>
</xsl:text>
</xsl:if>
<xsl:if test="position() != last()">
<xsl:text>    </xsl:text>[and]
</xsl:if>
</xsl:for-each >
</xsl:otherwise>
</xsl:choose>

  [Back to top](#top)
  </xsl:for-each>

  
</xsl:template>
</xsl:stylesheet>

