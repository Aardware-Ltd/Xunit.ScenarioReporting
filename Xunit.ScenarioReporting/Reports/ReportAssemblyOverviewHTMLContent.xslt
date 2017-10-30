<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xlink="http://www.w3.org/1999/xlink">
  <xsl:output method="html" version="1.0" encoding="UTF-8" indent="yes" omit-xml-declaration="yes"/>

    <xsl:template name="data-driven-toc">
      <ul class="menu">
        <xsl:for-each select="Assembly/Definition">
          <xsl:sort select="Grouping"/>
          <xsl:variable name="lcase-grouping" select="translate(translate(Grouping, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '.', '-')"></xsl:variable>          
          <li class="menu-item">
            <xsl:attribute name="class">
              <xsl:text>menu-item </xsl:text>
              <xsl:choose>
                <xsl:when test="descendant::Failure">
                  <xsl:text>ht-fail </xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>ht-success </xsl:text>
                </xsl:otherwise>
              </xsl:choose>
              <xsl:value-of select="concat('ht-', $lcase-grouping)"/>
            </xsl:attribute>            
            <a href="#{generate-id(Name)}">
              <xsl:value-of select="Name" />
            </a>
            <xsl:if test="descendant::Failure">
              <xsl:text> (</xsl:text>
              <span class="status-failure">
                <xsl:text>Failure</xsl:text>
              </span>
              <xsl:text>)</xsl:text>
            </xsl:if>
          </li>
        </xsl:for-each>
      </ul>
    </xsl:template>
  
    <xsl:template name="data-driven-filter-gui-input">
    <xsl:for-each select="Assembly/Definition/Grouping[not(.=preceding::*)]">
      <xsl:sort select="." />
      <xsl:variable name="lcase-grouping" select="translate(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '.', '-')"></xsl:variable>
      <input class="menu-hashtag" type="radio" name="filter" id="filter-">
        <xsl:attribute name="id">
          <xsl:value-of select="concat('filter-', $lcase-grouping)"/>
        </xsl:attribute>
      </input>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="data-driven-filter-gui-label">
    <xsl:for-each select="Assembly/Definition/Grouping[not(.=preceding::*)]">
      <xsl:sort select="." />
      <xsl:variable name="lcase-grouping" select="translate(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '.', '-')"></xsl:variable>
      <label for="filter-" class="class">
        <xsl:attribute name="for">
          <xsl:value-of select="concat('filter-', $lcase-grouping)"/>
        </xsl:attribute>
        <xsl:text>#</xsl:text><xsl:value-of select="."/>
      </label>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="data-driven-filter-gui-css">
    <xsl:comment>Data-driven CSS.</xsl:comment>
    <!--Example: #filter-coffeeshoptests:checked ~ .wrapper label[for="filter-coffeeshoptests"]-->
    <style>
      <xsl:text>/*Toggle appearance of dynamic hashtag filter menu when checked*/</xsl:text>
      <xsl:for-each select="Assembly/Definition/Grouping[not(.=preceding::*)]">
        <xsl:sort select="." />
        <xsl:variable name="lcase-grouping" select="translate(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '.', '-')"></xsl:variable>
        <xsl:text>#filter-</xsl:text>
        <xsl:value-of select="$lcase-grouping"/>
        <xsl:text>:checked ~ .wrapper label[for="filter-</xsl:text>
        <xsl:value-of select="$lcase-grouping"/>
        <xsl:text>"]</xsl:text>
        <xsl:if test="position() != last()">
          <xsl:text>, </xsl:text>
        </xsl:if>
      </xsl:for-each>
      <xsl:text>
      {
        background-color: #55da73;
        box-shadow: inset 2px 2px 1px 0px #6b7b5f;
      }
      </xsl:text>
    </style>
  </xsl:template>

  <xsl:template name="data-driven-filter-transition-CSS">
    <xsl:comment>Data-driven CSS.</xsl:comment>
    <!--Example: #filter-coffeeshoptests:checked ~ .wrapper .report-body > .ht-coffeeshoptests-->
    <style>
      <xsl:text>/*Transition elements - Show*/</xsl:text>
      <xsl:for-each select="Assembly/Definition/Grouping[not(.=preceding::*)]">
        <xsl:sort select="." />
        <xsl:variable name="lcase-grouping" select="translate(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '.', '-')"></xsl:variable>
        <xsl:text>#filter-</xsl:text>
        <xsl:value-of select="$lcase-grouping"/>
        <xsl:text>:checked ~ .wrapper .report-body > .ht-</xsl:text>
        <xsl:value-of select="$lcase-grouping"/>
        <xsl:if test="position() != last()">
          <xsl:text>, </xsl:text>
        </xsl:if>
      </xsl:for-each>
      <xsl:text>
      {
            position: static;
            transition-timing-function: ease-in;
            transition: opacity 1.8s;
            visibility: visible;
            opacity: 1;
      }
      </xsl:text>
    </style>
  </xsl:template>

    <xsl:template name="data-driven-toc-transition-CSS">
    <xsl:comment>Data-driven CSS.</xsl:comment>
    <!--Example: #filter-coffeeshoptests:checked ~ .wrapper .report-header .menu > .ht-coffeeshoptests -->
    <style>
      <xsl:text>/*Transition elements - Show*/</xsl:text>
      <xsl:for-each select="Assembly/Definition/Grouping[not(.=preceding::*)]">
        <xsl:sort select="." />
        <xsl:variable name="lcase-grouping" select="translate(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '.', '-')"></xsl:variable>
        <xsl:text>#filter-</xsl:text>
        <xsl:value-of select="$lcase-grouping"/>
        <xsl:text>:checked ~ .wrapper .report-header .menu > .ht-</xsl:text>
        <xsl:value-of select="$lcase-grouping"/>
        <xsl:if test="position() != last()">
          <xsl:text>, </xsl:text>
        </xsl:if>
      </xsl:for-each>
      <xsl:text>
      {
            position: static;
            transition-timing-function: ease-in;
            transition: opacity 1.8s;
            visibility: visible;
            opacity: 1;
      }
      </xsl:text>
    </style>
  </xsl:template>

    <xsl:template match="Given">
    <div class="section-given">
      <xsl:apply-templates select="Details"></xsl:apply-templates>
    </div>
    <!--section-given-->
  </xsl:template>

  <xsl:template match="When">
    <div class="section-when">
      <xsl:apply-templates select="Details"></xsl:apply-templates>
    </div>
    <!--section-when-->
  </xsl:template>

  <xsl:template match="Then">
    <div class="section-then">
      <xsl:apply-templates select="Details"></xsl:apply-templates>
    </div>
    <!--section-then-->
  </xsl:template>

  <xsl:template match="Details">
    <xsl:choose>
      <xsl:when test="name(..) = 'Details'">
        <xsl:if test="count(preceding-sibling::Details) = 0">
          <div class="arrow">&#160;</div>
          <!--Needs content to prevent any following markup being insterted withing this div. Reason unknown.-->
        </xsl:if>
        <div class="nested-child">

          <p>
            <xsl:value-of select="Title"/>
          </p>
          <xsl:call-template name="WriteDetails" />

        </div>
      </xsl:when>
      <xsl:otherwise>

        <p>
          <xsl:value-of select="Title"/>
        </p>

        <xsl:call-template name="WriteDetails" />
      </xsl:otherwise>
    </xsl:choose>

    <!--nested-child-->
  </xsl:template>
  <xsl:template name="WriteDetails">
    <xsl:choose>
      <xsl:when test="@show='False'">
        <xsl:apply-templates select="Details"></xsl:apply-templates>
        <xsl:apply-templates select="Detail"></xsl:apply-templates>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="Details[@show='True']"></xsl:apply-templates>
        <xsl:apply-templates select="Detail[@show='True']"></xsl:apply-templates>
        <xsl:apply-templates select ="Failure"></xsl:apply-templates>

        <xsl:if test="@show='True' and (Details/@show = 'False' or Detail/@show = 'False')">
          <div class="accordion tech">
            <div class="tab">
              <input id="tab-tech-{generate-id(current())}-{position()}" type="checkbox" name="tabs" />
              <label for="tab-tech-{generate-id(current())}-{position()}">Tech Fields</label>
              <div class="tab-content">
                <xsl:apply-templates select="Detail[@show='False']"></xsl:apply-templates>
                <xsl:apply-templates select="Details[@show='False']"></xsl:apply-templates>
              </div>
            </div>
          </div>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Detail">
    <xsl:if test="not(descendant::Failure)">
      <p>
        <xsl:text>with </xsl:text>
        <code>
          <xsl:value-of select="Name"/>
        </code>
        <xsl:text> </xsl:text>
        <xsl:value-of select="Value"/>
      </p>
    </xsl:if>
    <xsl:if test="descendant::Failure">
      <xsl:apply-templates select ="Failure"></xsl:apply-templates>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Mismatch">
    <div class="status-failure">
      <div class="accordion error">
        <div class="tab">
          <input id="tab-fail-{generate-id(current())}-{position()}" type="checkbox" name="tabs" />
          <label for="tab-fail-{generate-id(current())}-{position()}">
            Failure: <span class="fail-message">
              <xsl:value-of select="Name"/> Mismatch
            </span>
          </label>
          <div class="tab-content">
            <ul>
              <li>
                <xsl:text>Expected: </xsl:text>
                <xsl:value-of select="Expected/Value"/>
              </li>
              <li>
                <xsl:text>Actual: </xsl:text>
                <xsl:value-of select="Actual/Value"/>
              </li>
            </ul>
          </div>
          <!--tab-content -->
          <svg class="icon icon-error-x">
            <use xlink:href="#icon-error-x"></use>
          </svg>
        </div>
        <!--tab -->
      </div>
      <!--accordion -->
    </div>
    <!--status-failure -->
  </xsl:template>

  <xsl:template match="Exception">
    <div class="status-failure">
      <div class="accordion error">
        <div class="tab">
          <input id="tab-fail-{generate-id(current())}-{position()}" type="checkbox" name="tabs" />
          <label for="tab-fail-{generate-id(current())}-{position()}">
            <xsl:text>Failure: </xsl:text>
            <span class="fail-message">
              <code>
                <xsl:value-of select="Name"/>
              </code>
              <xsl:text> </xsl:text>
              <xsl:value-of select="Type"/>
            </span>
          </label>
          <div class="tab-content">
            <p>
              <xsl:value-of select="Value"/>
            </p>
          </div>
          <!--tab-content -->
          <svg class="icon icon-error-x">
            <use xlink:href="#icon-error-x"></use>
          </svg>
        </div>
        <!--tab -->
      </div>
      <!--accordion -->
    </div>
    <!--status-failure -->
  </xsl:template>

  <xsl:template match="/">

    <xsl:call-template name="data-driven-filter-gui-css"></xsl:call-template>
    <xsl:call-template name="data-driven-filter-transition-CSS"></xsl:call-template>
    <xsl:call-template name="data-driven-toc-transition-CSS"></xsl:call-template>

    <input class="menu-hashtag" type="radio" name="filter" id="filter-all" checked="checked"></input>
    <input class="menu-hashtag" type="radio" name="filter" id="filter-success"></input>
    <input class="menu-hashtag" type="radio" name="filter" id="filter-fail"></input>

    <xsl:call-template name="data-driven-filter-gui-input"></xsl:call-template>

    <div class="wrapper report-assembly">

      <div class="report-header">
        <h2>
          <a name="top">
            <xsl:text>Here's what happened</xsl:text>
          </a>
        </h2>
        <p>
          <xsl:text>Looking at Assembly </xsl:text>
          <span class="assembly-name">
            <xsl:value-of select="Assembly/Name"/>
          </span>
          <xsl:text> of </xsl:text>
          <span class="assembly-timestamp">
            <xsl:value-of select="Assembly/Time"/>
          </span>
          <xsl:text> with the following Scenarios.</xsl:text>
        </p>
        <xsl:variable name="cntDefinition" select="count(Assembly/Definition)" />
        <p>
          <xsl:value-of select="$cntDefinition" />
          <xsl:text> Scenario(s):</xsl:text>
        </p>
        <xsl:if test="$cntDefinition = 0">
          <p>
            <xsl:text>[There are no Scenarios.]</xsl:text>
          </p>
        </xsl:if>
        <xsl:if test="$cntDefinition > 0">

          <xsl:call-template name="data-driven-toc"></xsl:call-template>
        
          </xsl:if>

        <div class="menu-hashtag">
          <label for="filter-all">#all</label>
          <label for="filter-success">#success</label>
          <label for="filter-fail">#fail</label>
        </div>
        <div class="menu-hashtag data-driven">
          <xsl:call-template name="data-driven-filter-gui-label"></xsl:call-template>
        </div>        
        
        
        
      </div>
      <!--report-header-->

      <div class="report-body">

        <xsl:for-each select="Assembly/Definition">

          <xsl:sort select="Name"/>

          <a name="{generate-id(Name)}"></a>
          <xsl:variable name="hashtag-grouping" select="concat('ht-', translate(translate(Grouping, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '.', '-'))"></xsl:variable>
          <section class="section-scenario">
            <xsl:choose>
              <xsl:when test="descendant::Failure">
                <xsl:attribute name="class"><xsl:text>section-scenario status-failure ht-fail </xsl:text><xsl:value-of select="$hashtag-grouping"/></xsl:attribute>
                <svg class="icon icon-lemon-2 jello animated">
                  <use xlink:href="#icon-lemon-2"></use>
                </svg>
                <!--::THERE IS A FAILURE::-->
              </xsl:when>
              <xsl:otherwise>
                <xsl:attribute name="class"><xsl:text>section-scenario ht-success </xsl:text><xsl:value-of select="$hashtag-grouping"/></xsl:attribute>
              </xsl:otherwise>
            </xsl:choose>

            <h3>
              <xsl:text>Scenario</xsl:text>
            </h3>
            <p>
              <xsl:text>Name: </xsl:text>
              <xsl:value-of select="Name"/>
            </p>
            <p>
              <xsl:text>Nom de guerre: </xsl:text>
              <xsl:value-of select="NDG"/>
            </p>
            <div class="section-givens">
              <h4>
                <xsl:text>Given</xsl:text>
              </h4>
              <xsl:variable name="cntGivens" select="count(Given)" />
              <xsl:choose>
                <xsl:when test="$cntGivens = 0">
                  <p>
                    <xsl:text>[There are no Givens]</xsl:text>
                  </p>
                </xsl:when>
                <xsl:otherwise>

                  <xsl:apply-templates select ="Given"></xsl:apply-templates>

                </xsl:otherwise>
              </xsl:choose>
            </div>
            <!--section-givens-->

            <div class="section-whens">
              <h4>
                <xsl:text>When</xsl:text>
              </h4>
              <xsl:variable name="cntWhen" select="count(When)" />
              <xsl:choose>
                <xsl:when test="$cntWhen = 0">
                  <p>
                    <xsl:text>[There is no When]</xsl:text>
                  </p>
                  <!--There is no spoon-->
                </xsl:when>
                <xsl:otherwise>

                  <xsl:apply-templates select ="When"></xsl:apply-templates>

                </xsl:otherwise>
              </xsl:choose>
            </div>
            <!--section-whens-->

            <div class="section-thens">
              <h4>Then</h4>
              <xsl:variable name="cntThen" select="count(Then)" />
              <xsl:choose>
                <xsl:when test="$cntThen = 0">
                  <p>[There are no Thens]</p>
                </xsl:when>
                <xsl:otherwise>

                  <xsl:apply-templates select ="Then"></xsl:apply-templates>

                </xsl:otherwise>
              </xsl:choose>
            </div>
            <!--section-thens-->

            <div class="accordion tech">
              <div class="tab">
                <input id="tab-tech-{generate-id(current())}-{position()}" type="checkbox" name="tabs" />
                <label for="tab-tech-{generate-id(current())}-{position()}">Nerd Area</label>
                <div class="tab-content">

                  <xsl:if test="count(Scope) > 0">
                    <p>Scenario Scope</p>
                    <ul>
                      <xsl:for-each select="Scope">
                        <li>
                          <xsl:value-of select="."/>
                        </li>
                      </xsl:for-each>
                    </ul>
                  </xsl:if>
                  <xsl:if test="count(Then/Scope) > 0">
                    <p>Then Scope(s)</p>
                    <ul>
                      <xsl:for-each select="Then/Scope">
                        <li>
                          <xsl:value-of select="."/>
                        </li>
                      </xsl:for-each>
                    </ul>
                  </xsl:if>
                </div>
                <!--tab-content -->
              </div>
              <!--tab-->
            </div>
            <!--tech-->


            <p>
              <a href="#top">Back to top</a>
            </p>

          </section>
          <!--section-scenario-->
        </xsl:for-each>

      </div>
      <!--report-body-->
    </div>
    <!--report-assembly-->

  </xsl:template>


</xsl:stylesheet>