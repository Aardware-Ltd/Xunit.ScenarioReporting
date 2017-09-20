<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output version="1.0" encoding="UTF-8" indent="no" omit-xml-declaration="yes"/>
  <xsl:strip-space elements="*" />

  <xsl:template match="Given|When|Then">
    <xsl:value-of select="Title"/>
    <xsl:text>&#xd;&#xd;</xsl:text>
    <xsl:if test="not(descendant::Failure)">
      <xsl:apply-templates select="Detail"></xsl:apply-templates>
    </xsl:if>
    <xsl:apply-templates select ="Detail/Failure"></xsl:apply-templates>
    <xsl:if test="position() != last()">
      <xsl:text>&#xd;&#xd;[and]&#xd;&#xd;</xsl:text>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Child">
    <!--<xsl:text>[In Child node. Calling indent on count Ancestor Detail=</xsl:text><xsl:value-of select="count(ancestor::Detail)"/><xsl:text>]</xsl:text><xsl:text>&#xd;&#xd;</xsl:text>-->
    <xsl:call-template name="indent">
      <xsl:with-param name="n" select="count(ancestor::Detail)"/>
    </xsl:call-template>
    <xsl:value-of select="Title"/><!--<xsl:text>[A Title inside a Child node. Ancestor Child=</xsl:text><xsl:value-of select="count(ancestor::Child)"/><xsl:text> Ancestor Detail=</xsl:text><xsl:value-of select="count(ancestor::Detail)"/><xsl:text>]</xsl:text>-->
    <xsl:text>&#xd;&#xd;</xsl:text>
    <xsl:apply-templates select="Detail"></xsl:apply-templates>
  </xsl:template>

  <xsl:template match="Detail">
    <!--<xsl:text>[In Detail node. Calling indent on count Ancestor Detail=</xsl:text><xsl:value-of select="count(ancestor::Detail)"/><xsl:text>]</xsl:text><xsl:text>&#xd;&#xd;</xsl:text>-->
    <xsl:call-template name="indent">
      <xsl:with-param name="n" select="count(ancestor::Detail)"/>
    </xsl:call-template>
    <xsl:text> with </xsl:text>
    <xsl:value-of select="Name"/>
    <xsl:text> </xsl:text>
    <xsl:value-of select="Value"/><!--<xsl:text>[A Name-Value pair inside a Detail node. Ancestor Child=</xsl:text><xsl:value-of select="count(ancestor::Child)"/><xsl:text> Ancestor Detail=</xsl:text><xsl:value-of select="count(ancestor::Detail)"/><xsl:text>]</xsl:text>-->
    <xsl:text>&#xd;&#xd;</xsl:text>
    <xsl:apply-templates select="Child"></xsl:apply-templates>
  </xsl:template>

  <xsl:template match="Mismatch">
    <xsl:text>&#xd;&#xd;</xsl:text>
    <xsl:text>```&#xd;&#xd;</xsl:text>
    <xsl:value-of select="Name"/>
    <xsl:text> Mismatch:&#xd;</xsl:text>
    <xsl:text>* Expected: </xsl:text>
    <xsl:value-of select="Expected/Value"/>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text>* Actual: </xsl:text>
    <xsl:value-of select="Actual/Value"/>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text>&#xd;```</xsl:text>
    <xsl:text>&#xd;&#xd;```::FAILURE::```&#xd;&#xd;</xsl:text>
  </xsl:template>

  <xsl:template match="Scope">
    <xsl:text>* </xsl:text>
    <xsl:value-of select="."/>
    <xsl:text>&#xd;</xsl:text>
  </xsl:template>

  <xsl:template name="indent">
      <xsl:param name="n"/>
      <xsl:if test="$n > 0">                              
          <xsl:call-template name="indent">                
              <xsl:with-param name="n" select="$n - 1"/>  <!-- recurse n-1 -->
          </xsl:call-template>
          <xsl:text>>&#xA0;&#xA0;&#xA0;   </xsl:text>                  
      </xsl:if>
  </xsl:template>
  
  <xsl:template match="/">
    <a name="top">
      <xsl:text>&#xd;</xsl:text>
    </a>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text>## Here's what happened</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text>Looking at Assembly **</xsl:text>
    <xsl:value-of select="Assembly/Name"/>
    <xsl:text>** of **</xsl:text>
    <xsl:value-of select="Assembly/Time"/>
    <xsl:text>** with the following Scenarios.</xsl:text>
    <xsl:text>&#xd;</xsl:text>

    <xsl:variable name="cntDefinition" select="count(Assembly/Definition)" />

    <xsl:value-of select="$cntDefinition" />
    <xsl:text> Scenarios(s):</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:if test="$cntDefinition = 0">
      <xsl:text>[There are no Scenarios.]</xsl:text>
      <xsl:text>&#xd;</xsl:text>
    </xsl:if>

    <xsl:if test="$cntDefinition > 0">
      <xsl:for-each select="Assembly/Definition">
        <xsl:variable name="navID" select="generate-id(Name)" />
        <xsl:text>* [</xsl:text>
        <xsl:value-of select="Name" />
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
      <a name="{generate-id(Name)}">
        <xsl:text>&#xd;</xsl:text>
      </a>

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

      <xsl:text>Name: </xsl:text>
      <xsl:value-of select="Name"/>
      <xsl:text>&#xd;</xsl:text>
      <xsl:text>&#xd;</xsl:text>
      <xsl:text>Nom de guerre: </xsl:text>
      <xsl:value-of select="NDG"/>
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

          <xsl:apply-templates select ="Given"></xsl:apply-templates>
        
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

          <xsl:apply-templates select ="When"></xsl:apply-templates>
        
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

          <xsl:apply-templates select ="Then"></xsl:apply-templates>
        
        </xsl:otherwise>
      </xsl:choose>

      <xsl:text>&#xd;&#xd;</xsl:text>

      <xsl:text>##### Nerd Area</xsl:text>
      <xsl:text>&#xd;&#xd;</xsl:text>
      <xsl:if test="count(Scope) > 0">
        <xsl:text>&#xd;&#xd;</xsl:text>
        <xsl:text>Scenario Scope</xsl:text>
        <xsl:text>&#xd;&#xd;</xsl:text>
        <xsl:apply-templates select ="Scope"></xsl:apply-templates>
      </xsl:if>
      <xsl:if test="count(Then/Scope) > 0">
        <xsl:text>&#xd;&#xd;</xsl:text>
        <xsl:text>Then Scope(s)</xsl:text>
        <xsl:text>&#xd;&#xd;</xsl:text>
        <xsl:apply-templates select ="Then/Scope"></xsl:apply-templates>
      </xsl:if>


      <xsl:text>&#xd;&#xd;[Back to top](#top)&#xd;&#xd;</xsl:text>

      <xsl:text>---</xsl:text>
      <xsl:text>&#xd;&#xd;</xsl:text>

    </xsl:for-each>


  </xsl:template>
</xsl:stylesheet>

