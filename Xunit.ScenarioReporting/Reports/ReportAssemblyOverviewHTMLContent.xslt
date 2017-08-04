<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output version="1.0" encoding="UTF-8" indent="yes" omit-xml-declaration="yes"/>
<xsl:template match="/">
		<div class="report-assembly">

		<div class="report-header">
        <h2><a name="top">Here's what happened</a></h2>
        <p>Looking at Assembly <span class="assembly-name"><xsl:value-of select="Assembly/Name"/></span> of <span class="assembly-timestamp"><xsl:value-of select="Assembly/Time"/></span> with the following Scenarios.</p>
        <xsl:variable name="cntScenario" select="count(Assembly/Scenario)" />
		<p><xsl:value-of select="$cntScenario" /> Scenario(s):</p>
		<xsl:if test="$cntScenario = 0">
			<p>[There are no scenarios.]</p>
    </xsl:if>
		
		<xsl:if test="$cntScenario > 0">
			<ul>
				<xsl:for-each select="Assembly/Scenario">
					<li>
						<a href="#{generate-id(Name)}">
						<xsl:value-of select="Name" /></a>
					</li>
				</xsl:for-each>
			</ul>
    </xsl:if>
		
		</div><!--report-header-->

		<div class="report-body">

			<xsl:for-each select="Assembly/Scenario">

        <section class="section-scenario">
				<h3><a name="{generate-id(Name)}">Scenario</a></h3>
				<p>Name: <xsl:value-of select="Name"/></p>

				<div class="section-givens">
				<h4>Given</h4>
				<xsl:variable name="cntGivens" select="count(Given)" />
				<xsl:choose>
				<xsl:when test="$cntGivens = 0">
					<p>[There are no Givens]</p>
				</xsl:when>
				<xsl:otherwise>
					<xsl:for-each select="Given">
						<div class="section-given">
						<p><xsl:value-of select="Title"/></p><!--Title-->
						<xsl:if test="Detail != ''" >
							<p><xsl:for-each select="Detail/Message"><xsl:value-of select="concat(., ' ')"/></xsl:for-each ></p><!--Message-->
						</xsl:if>						
						</div><!--section-given-->
            <!--<xsl:if test="position() != last()"> [and] </xsl:if>-->
          </xsl:for-each >
				</xsl:otherwise>
				</xsl:choose>
				</div><!--section-givens-->
				
				<div class="section-whens">
				<h4>When</h4>
				<xsl:variable name="cntWhen" select="count(When)" />
				<xsl:choose>
				<xsl:when test="$cntWhen = 0">
					<p>[There is no When]</p> <!--There is no spoon-->
				</xsl:when>
				<xsl:otherwise>
					<xsl:for-each select="When">
						<div class="section-when">
						<p><xsl:value-of select="Title"/></p><!--Title-->
						<xsl:if test="Detail != ''" >
							<p><xsl:for-each select="Detail/Message"><xsl:value-of select="concat(., ' ')"/></xsl:for-each ></p><!--Message-->
						</xsl:if>						
						</div><!--section-when-->
					</xsl:for-each >				
				</xsl:otherwise>
				</xsl:choose>				
				</div><!--section-whens-->

				<div class="section-thens">
				<h4>Then</h4>
				<xsl:variable name="cntThen" select="count(Then)" />
				<xsl:choose>
				<xsl:when test="$cntThen = 0">
					<p>[There are no Whens]</p>
				</xsl:when>
				<xsl:otherwise>
					<xsl:for-each select="Then">
						<div class="section-then">					
						<p><xsl:value-of select="Title"/></p><!--Title-->
						<xsl:if test="Detail != ''" >
							<p><xsl:for-each select="Detail/Message"><xsl:value-of select="concat(., ' ')"/></xsl:for-each ></p><!--Message-->
						</xsl:if>						
						</div><!--section-then-->
					</xsl:for-each >
				</xsl:otherwise>
				</xsl:choose>
				</div><!--section-thens-->
				<p><a href="#top">Back to top</a></p>
        </section><!--section-scenario-->
    </xsl:for-each>

		</div><!--report-body-->
		</div><!--report-assembly-->
  
</xsl:template>
</xsl:stylesheet>

