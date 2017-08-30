<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output version="1.0" encoding="UTF-8" indent="no" omit-xml-declaration="yes"/>
  <xsl:strip-space elements="*" />
  <xsl:template match="/">
    <xsl:text>## </xsl:text><a name="top"><xsl:text>Here's what happened</xsl:text></a>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text>Looking at Assembly **</xsl:text><xsl:value-of select="Assembly/Name"/><xsl:text>** of **</xsl:text><xsl:value-of select="Assembly/Time"/><xsl:text>** with the following Scenarios.</xsl:text>
    <xsl:text>&#xd;</xsl:text>

    <xsl:variable name="cntDefinition" select="count(Assembly/Definition)" />
  
    <xsl:value-of select="$cntDefinition" /><xsl:text> Scenarios(s):</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:if test="$cntDefinition = 0">
      <xsl:text>[There are no Scenarios.]</xsl:text>
      <xsl:text>&#xd;</xsl:text>
    </xsl:if>
		
    <xsl:if test="$cntDefinition > 0">
      <xsl:for-each select="Assembly/Definition">
        <xsl:variable name="navID" select="generate-id(Name)" />
        <xsl:text>* [</xsl:text><xsl:value-of select="Name" />
        <xsl:text>](&#35;</xsl:text>
        <xsl:value-of select="$navID" />
        <xsl:text>)</xsl:text>
        <xsl:if test="descendant::Failure">
          <xsl:text> (```FAILURE```)</xsl:text>
        </xsl:if>
        <xsl:text>&#xd;</xsl:text>
      </xsl:for-each>
      <xsl:text>&#xd;</xsl:text>
    </xsl:if>

    <xsl:text>&#xa;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text>&#xa;</xsl:text>
    <xsl:text>&#xd;</xsl:text>

    <xsl:text>---</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text>&#xd;</xsl:text>

    <xsl:for-each select="Assembly/Definition">
      <a name="{generate-id(Name)}"><xsl:text>&#xd;</xsl:text></a>
        
      <xsl:text>&#xd;</xsl:text>
      <xsl:text>&#xd;</xsl:text>

      <xsl:if test="descendant::Failure">
        <xsl:text>```::FAILURE::```</xsl:text>
        <xsl:text>&#xd;</xsl:text>
        <xsl:text>&#xd;</xsl:text>
        <!--::THERE IS A FAILURE::-->
      </xsl:if>



      <xsl:text>### Scenario</xsl:text>
      <xsl:text>&#xd;</xsl:text>
  
      <xsl:text>Name: </xsl:text><xsl:value-of select="Name"/>
      <xsl:text>&#xd;</xsl:text>
      <xsl:text>&#xd;</xsl:text>
      <xsl:text>Nom de guerre: </xsl:text><xsl:value-of select="NDG"/>
      <xsl:text>&#xd;</xsl:text>
      <xsl:text>&#xd;</xsl:text>

      <xsl:text>#### Given</xsl:text>
      <xsl:text>&#xd;</xsl:text>
      <xsl:variable name="cntGivens" select="count(Given)" />
      <xsl:choose>
        <xsl:when test="$cntGivens = 0">
          <xsl:text>[There are no Givens]</xsl:text>
          <xsl:text>&#xd;</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:for-each select="Given">
            <xsl:value-of select="Title"/><!--Title-->
            <xsl:if test="Detail != ''" >
              <xsl:text> with </xsl:text>
              <xsl:for-each select="Detail/.">
                <xsl:value-of select="Name"/>
                <xsl:text> </xsl:text>
                <xsl:value-of select="Value"/>
              </xsl:for-each>
            </xsl:if>
            <xsl:if test="position() != last()">
              <xsl:text>&#xd;</xsl:text>
              <xsl:text>&#xd;</xsl:text>
              <xsl:text>[and]</xsl:text>
              <xsl:text>&#xd;</xsl:text>
              <xsl:text>&#xd;</xsl:text>
            </xsl:if>
          </xsl:for-each>				
        </xsl:otherwise>
      </xsl:choose>

      <xsl:text>&#xd;</xsl:text>
      <xsl:text>&#xd;</xsl:text>

      <xsl:text>#### When</xsl:text>
      <xsl:text>&#xd;</xsl:text>
      <xsl:variable name="cntWhen" select="count(When)" />
      <xsl:choose>
        <xsl:when test="$cntWhen = 0">
          <xsl:text>[There is no When]</xsl:text>
          <xsl:text>&#xd;</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:for-each select="When">
            <xsl:value-of select="Title"/><!--Title-->
            <xsl:if test="Detail != ''" >
              <xsl:text> with </xsl:text>
              <xsl:for-each select="Detail/.">
                <xsl:value-of select="Name"/>
                <xsl:text> </xsl:text>
                <xsl:value-of select="Value"/>
              </xsl:for-each>
            </xsl:if>
            <xsl:if test="position() != last()">
              <xsl:text>&#xd;</xsl:text>
              <xsl:text>&#xd;</xsl:text>
              <xsl:text>[and]</xsl:text>
              <xsl:text>&#xd;</xsl:text>
              <xsl:text>&#xd;</xsl:text>
            </xsl:if>
          </xsl:for-each>				
        </xsl:otherwise>
      </xsl:choose>

      <xsl:text>&#xd;</xsl:text>
      <xsl:text>&#xd;</xsl:text>

      <xsl:text>#### Then</xsl:text>
      <xsl:text>&#xd;</xsl:text>
      <xsl:variable name="cntThen" select="count(Then)" />
      <xsl:choose>
        <xsl:when test="$cntThen = 0">
          <xsl:text>[There are no Whens]</xsl:text>
          <xsl:text>&#xd;</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:for-each select="Then">
            <xsl:value-of select="Title"/><!--Title-->
            <xsl:text>&#xd;</xsl:text>
            <xsl:text>&#xd;</xsl:text>
            <xsl:choose>
            
              <xsl:when test="count(Detail/Failure) > 0">
                  <xsl:for-each select="Detail/Failure/Mismatch">
                    <xsl:text>```</xsl:text>
                    <xsl:text>&#xd;</xsl:text>
                    <xsl:text>&#xd;</xsl:text>
                    <xsl:value-of select="Name"/>
                    <xsl:text> Mismatch:</xsl:text>
                    <xsl:text>&#xd;</xsl:text>
                    <xsl:text>* Expected: </xsl:text><xsl:value-of select="Expected/Value"/>
                    <xsl:text>&#xd;</xsl:text>
                    <xsl:text>* Actual: </xsl:text><xsl:value-of select="Actual/Value"/>
                    <xsl:text>&#xd;</xsl:text>
                    <xsl:text>&#xd;</xsl:text>
                    <!--::FAILURE::-->
                    <xsl:text>```</xsl:text>
                    <xsl:text>&#xd;</xsl:text>
                    <xsl:text>```::FAILURE::```</xsl:text>
                    <xsl:text>&#xd;</xsl:text>
                    <xsl:text>&#xd;</xsl:text>
                  </xsl:for-each >
              </xsl:when>
              <xsl:otherwise>
                <xsl:if test="count(Detail/.) > 0" >
                  <xsl:text> with </xsl:text>
                  <xsl:for-each select="Detail/.">
                    <xsl:value-of select="Name"/>
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="Value"/>
                  </xsl:for-each>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>

            <xsl:text>&#xd;</xsl:text>

            <xsl:if test="position() != last()">
              <xsl:text>&#xd;</xsl:text>
              <xsl:text>&#xd;</xsl:text>
              <xsl:text>[and]</xsl:text>
              <xsl:text>&#xd;</xsl:text>
              <xsl:text>&#xd;</xsl:text>
            </xsl:if>
          </xsl:for-each>
        </xsl:otherwise>
      </xsl:choose>

      <xsl:text>&#xd;</xsl:text>
      <xsl:text>&#xd;</xsl:text>
      <xsl:text>[Back to top](#top)</xsl:text>
      <xsl:text>&#xd;</xsl:text>
      <xsl:text>&#xd;</xsl:text>
      <xsl:text>&#xd;</xsl:text>
      
      <xsl:text>---</xsl:text>
      <xsl:text>&#xd;</xsl:text>
      <xsl:text>&#xd;</xsl:text>

    </xsl:for-each>


  </xsl:template>
</xsl:stylesheet>

