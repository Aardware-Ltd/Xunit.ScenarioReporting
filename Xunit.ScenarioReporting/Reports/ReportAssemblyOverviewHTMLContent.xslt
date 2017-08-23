<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xlink="http://www.w3.org/1999/xlink">
<xsl:output version="1.0" encoding="UTF-8" indent="yes" omit-xml-declaration="yes"/>
<xsl:template match="/">
		<div class="report-assembly">

		<div class="report-header">
        <h2><a name="top">Here's what happened</a></h2>
        <p>Looking at Assembly <span class="assembly-name"><xsl:value-of select="Assembly/Name"/></span> of <span class="assembly-timestamp"><xsl:value-of select="Assembly/Time"/></span> with the following Definitions.</p>
        <xsl:variable name="cntDefinition" select="count(Assembly/Definition)" />
		<p><xsl:value-of select="$cntDefinition" /> Definition(s):</p>
		<xsl:if test="$cntDefinition = 0">
			<p>[There are no scenarios.]</p>
    </xsl:if>
		
		<xsl:if test="$cntDefinition > 0">
			<ul class="menu">
				<xsl:for-each select="Assembly/Definition">
					<li class="menu-item">
						<a href="#{generate-id(Name)}"><xsl:value-of select="Name" /></a><xsl:if test="descendant::Failure"> (<span class="status-failure">Failure</span>)</xsl:if>
					</li>
				</xsl:for-each>
			</ul>
    </xsl:if>
		
		</div><!--report-header-->

		<div class="report-body">

			<xsl:for-each select="Assembly/Definition">

        <a name="{generate-id(Name)}"></a>
        <section class="section-scenario">

        <xsl:if test="descendant::Failure">
          <xsl:attribute name="class">section-scenario status-failure</xsl:attribute>
          <svg class="icon icon-lemon-2"><use xlink:href="#icon-lemon-2"></use></svg>
          <!--::THERE IS A FAILURE::-->
        </xsl:if>

        <h3>Definition</h3>
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
							<p>with <xsl:for-each select="Detail/Message"><xsl:value-of select="concat(., ' ')"/></xsl:for-each ></p><!--Message-->
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
							<p>with <xsl:for-each select="Detail/Message"><xsl:value-of select="concat(., ' ')"/></xsl:for-each ></p><!--Message-->
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
            <xsl:if test="descendant::Failure">
              <xsl:attribute name="class">section-then status-failure</xsl:attribute>
            </xsl:if>
						<p><xsl:value-of select="Title"/></p><!--Title-->
              <xsl:if test="count(Detail/Message) > 0" >
                <p>with 
                  <xsl:for-each select="Detail/Message">
                    <xsl:value-of select="concat(., ' ')"/>
                  </xsl:for-each >
                </p>
              </xsl:if>

              <xsl:if test="count(Detail/Failure) > 0" >
                <p class="status-failure">
                  <xsl:for-each select="Detail/Failure">
                    <xsl:value-of select="concat(., ' ')"/>::FAILURE::
                  </xsl:for-each >
                </p>
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

