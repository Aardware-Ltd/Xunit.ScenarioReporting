<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xlink="http://www.w3.org/1999/xlink">
<xsl:output version="1.0" encoding="UTF-8" indent="yes" omit-xml-declaration="yes"/>
<xsl:template match="/">
		<div class="report-assembly">

		<div class="report-header">
        <h2><a name="top">Here's what happened</a></h2>
        <p>Looking at Assembly <span class="assembly-name"><xsl:value-of select="Assembly/Name"/></span> of <span class="assembly-timestamp"><xsl:value-of select="Assembly/Time"/></span> with the following Scenarios.</p>
        <xsl:variable name="cntDefinition" select="count(Assembly/Definition)" />
		<p><xsl:value-of select="$cntDefinition" /> Scenario(s):</p>
		<xsl:if test="$cntDefinition = 0">
			<p>[There are no Scenarios.]</p>
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
          <svg class="icon icon-lemon-2 jello animated"><use xlink:href="#icon-lemon-2"></use></svg>
          <!--::THERE IS A FAILURE::-->
        </xsl:if>

        <h3>Scenario</h3>
        <p>Name: <xsl:value-of select="Name"/></p>
        <p>Nom de guerre: <xsl:value-of select="NDG"/></p>
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
              <p>with 
              <xsl:for-each select="Detail/.">
                <xsl:value-of select="Name"/><xsl:text> </xsl:text><xsl:value-of select="Value"/>              
              </xsl:for-each>
              </p>
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
              <p>with 
              <xsl:for-each select="Detail/.">
                <xsl:value-of select="Name"/><xsl:text> </xsl:text><xsl:value-of select="Value"/>              
              </xsl:for-each>
              </p>
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
						<div class="section-then"><!--
            <xsl:if test="descendant::Failure">
              <xsl:attribute name="class">section-then status-failure</xsl:attribute>
            </xsl:if>-->
						<p><xsl:value-of select="Title"/></p><!--Title-->
  
              <xsl:choose>
                <xsl:when test="count(Detail/Failure) > 0">
                <div class="status-failure">
                  <div class="accordion error">
                    <div class="tab">
                      <xsl:for-each select="Detail/Failure/Mismatch">
                      
                        <input id="tab-fail-{generate-id(current())}-{position()}" type="checkbox" name="tabs" />
                        <label for="tab-fail-{generate-id(current())}-{position()}">Failure: <span class="fail-message"><xsl:value-of select="Name"/> Mismatch</span></label>
                        <div class="tab-content">
                          <ul>
                            <li>Expected: <xsl:value-of select="Expected/Value"/></li>
                            <li>Actual: <xsl:value-of select="Actual/Value"/></li>
                          </ul>
                        </div> <!--tab-content -->
                        <!--::FAILURE::-->
                        <svg class="icon icon-error-x"><use xlink:href="#icon-error-x"></use></svg>
                      </xsl:for-each >
                    
                    </div> <!--tab --> 
                  </div> <!--accordion -->                    
                </div> <!--status-failure -->
                </xsl:when>
                <xsl:otherwise>
                  <xsl:if test="count(Detail/.) > 0" >
                    <p>with  
                      <xsl:for-each select="Detail/.">
                        <xsl:value-of select="Name"/><xsl:text> </xsl:text><xsl:value-of select="Value"/>              
                      </xsl:for-each>
                    </p>
                  </xsl:if>
                </xsl:otherwise>              
              </xsl:choose>
              
            </div><!--section-then-->
					</xsl:for-each >
				</xsl:otherwise>
				</xsl:choose>
				</div><!--section-thens-->

        <div class="accordion tech">
          <div class="tab">
            <input id="tab-tech-{generate-id(current())}-{position()}" type="checkbox" name="tabs" />
            <label for="tab-tech-{generate-id(current())}-{position()}">Nerd Area</label>
            <div class="tab-content">

              <xsl:if test="count(Scope) > 0">
                <p>Scenario Scope</p>
                <ul>                
                <xsl:for-each select="Scope">
                  <li><xsl:value-of select="."/></li>            
                </xsl:for-each>
                </ul>
              </xsl:if>
              <xsl:if test="count(Then/Scope) > 0">
                <p>Then Scope(s)</p>
                <ul>                
                <xsl:for-each select="Then/Scope">
                  <li><xsl:value-of select="."/></li>
                </xsl:for-each>
                </ul>
              </xsl:if>              
            </div> <!--tab-content -->
          </div><!--tech-->
        </div><!--tech-->
          
          
				<p><a href="#top">Back to top</a></p>

        </section><!--section-scenario-->
    </xsl:for-each>

		</div><!--report-body-->
		</div><!--report-assembly-->

</xsl:template>


</xsl:stylesheet>

