<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xlink="http://www.w3.org/1999/xlink">
    <xsl:output version="1.0" encoding="UTF-8" indent="yes" omit-xml-declaration="yes"/>

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
                        <div class="arrow"></div>
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
        <html>
            <head>
                <META http-equiv="Content-Type" content="text/html; charset=utf-8" />

                <style>
                    @import url('https://fonts.googleapis.com/css?family=Raleway');

                    .color-primary-0 {
                    color: #F0F5EC
                    }
                    /* Main Primary color */
                    .color-primary-1 {
                    color: #CEE8B2
                    }

                    .color-primary-2 {
                    color: #DDEECB
                    }

                    .color-primary-3 {
                    color: #DFE7D6
                    }

                    .color-primary-4 {
                    color: #C8D2BE
                    }

                    .color-secondary-1-0 {
                    color: #FFFBF6
                    }
                    /* Main Secondary color (1) */
                    .color-secondary-1-1 {
                    color: #FFE6C4
                    }

                    .color-secondary-1-2 {
                    color: #FFEFD9
                    }

                    .color-secondary-1-3 {
                    color: #FFF7EC
                    }

                    .color-secondary-1-4 {
                    color: #E9E0D3
                    }

                    .color-secondary-2-0 {
                    color: #DAD7DE
                    }
                    /* Main Secondary color (2) */
                    .color-secondary-2-1 {
                    color: #9D91B7
                    }

                    .color-secondary-2-2 {
                    color: #B7AECA
                    }

                    .color-secondary-2-3 {
                    color: #AAA6B2
                    }

                    .color-secondary-2-4 {
                    color: #96929F
                    }

                    .color-complement-0 {
                    color: #F2EAED
                    }
                    /* Main Complement color */
                    .color-complement-1 {
                    color: #E3AEC1
                    }

                    .color-complement-2 {
                    color: #EAC7D3
                    }

                    .color-complement-3 {
                    color: #E1D0D6
                    }

                    .color-complement-4 {
                    color: #CCB9BF
                    }



                    body {
                    font-family: 'Raleway', sans-serif;
                    font-size: 16px;
                    background-color: #96929F;
                    }

                    code {
                    background: #F0F5EC;
                    padding: 0 5px 2px;
                    border-radius: 4px;
                    position: relative;
                    top: -1px;
                    margin: 0 3px 0 1px;
                    overflow-wrap: break-word;
                    word-wrap: break-word;
                    }

                    .report-assembly {
                    padding: 1em;
                    }

                    .report-header,
                    .report-header a {
                    color: antiquewhite;
                    }

                    .report-header {
                    font-size: 1.2rem;
                    }


                    .section-scenario {
                    margin: 2em auto;
                    padding: 1em;
                    background-color: #F0F5EC;
                    break-before: page;
                    }

                    .section-scenario li {
                    padding: .2em .1em;
                    }


                    .section-givens {
                    margin: 1em 3em 1em 1em;
                    padding: 1em;
                    background-color: #C8D2BE;
                    }

                    .section-whens {
                    margin: 1em 2em 1em 2em;
                    padding: 1em;
                    background-color: #DFE7D6;
                    }

                    .section-thens {
                    margin: 1em 1em 1em 3em;
                    padding: 1em;
                    background-color: #DDEECB;
                    }

                    .section-scenario,
                    .section-givens,
                    .section-whens,
                    .section-thens {
                    border-radius: 5px;
                    box-shadow: 0 10px 15px -10px #777;
                    overflow-wrap: break-word;
                    }

                    .section-scenario {
                    box-shadow: 0 20px 15px -15px #555;
                    }

                    .section-given:not(:last-child),
                    .section-when:not(:last-child),
                    .section-then:not(:last-child) {
                    border-bottom: 2px solid #F0F5EC;
                    }

                    .nested-child {
                    position-relative;
                    padding-left:4em;
                    }

                    .assembly-name,
                    .assembly-timestamp {
                    font-weight: bold;
                    }


                    .menu .menu-item .status-failure {
                    color: crimson;
                    }

                    .menu-item {
                    overflow-wrap: break-word;
                    }

                    .section-scenario.status-failure {
                    border-left: 0.4em solid crimson;
                    }

                    .section-then .status-failure {
                    border-left: 0.2em solid crimson;
                    padding-left: 0.5em;
                    }

                    .icon {
                    display: inline-block;
                    width: 1em;
                    height: 1em;
                    stroke-width: 0;
                    stroke: currentColor;
                    fill: currentColor;
                    }

                    .icon-lemon-2 {
                    font-size: 1.6rem;
                    }

                    .icon-error-x {
                    font-size: 3.2em;
                    }


                    .animated {
                    animation-duration: 1s;
                    animation-fill-mode: both;
                    }

                    @keyframes jello {
                    from, 11.1%, to {
                    transform: none;
                    }

                    22.2% {
                    transform: skewX(-12.5deg) skewY(-12.5deg);
                    }

                    33.3% {
                    transform: skewX(6.25deg) skewY(6.25deg);
                    }

                    44.4% {
                    transform: skewX(-3.125deg) skewY(-3.125deg);
                    }

                    55.5% {
                    transform: skewX(1.5625deg) skewY(1.5625deg);
                    }

                    66.6% {
                    transform: skewX(-0.78125deg) skewY(-0.78125deg);
                    }

                    77.7% {
                    transform: skewX(0.390625deg) skewY(0.390625deg);
                    }

                    88.8% {
                    transform: skewX(-0.1953125deg) skewY(-0.1953125deg);
                    }
                    }

                    @keyframes jello2 {
                    from, 11.1%, to {
                    transform: none;
                    }

                    22.2% {
                    transform: skewX(-12.5deg) skewY(-12.5deg);
                    }

                    33.3% {
                    transform: skewX(6.25deg) skewY(6.25deg);
                    }

                    44.4% {
                    transform: skewX(-3.125deg) skewY(-3.125deg);
                    }

                    55.5% {
                    transform: skewX(1.5625deg) skewY(1.5625deg);
                    }

                    66.6% {
                    transform: skewX(-0.78125deg) skewY(-0.78125deg);
                    }

                    77.7% {
                    transform: skewX(0.390625deg) skewY(0.390625deg);
                    }

                    88.8% {
                    transform: skewX(-0.1953125deg) skewY(-0.1953125deg);
                    }
                    }

                    .jello {
                    animation-name: jello;
                    transform-origin: center;
                    }
                    .jello:hover {
                    animation-name: jello2;
                    transform-origin: center;
                    }

                    /* Accordion styles */
                    .accordion .tab {
                    position: relative;
                    margin-bottom: 1px;
                    width: 100%;
                    color: #fff;
                    overflow: hidden;
                    }

                    .accordion input {
                    position: absolute;
                    opacity: 0;
                    z-index: -1;
                    }

                    .accordion label {
                    position: relative;
                    display: block;
                    padding: 0 0 0 1em;
                    background: #16a085;
                    font-weight: bold;
                    /*line-height: 3;*/
                    cursor: pointer;
                    }

                    .accordion .tab-content {
                    max-height: 0;
                    overflow: hidden;
                    background: #1abc9c;
                    -webkit-transition: max-height .35s;
                    -o-transition: max-height .35s;
                    transition: max-height .35s;
                    }
                    /* :checked */
                    .accordion input:checked ~ .tab-content {
                    max-height: 10em;
                    }
                    /* Icon */
                    .accordion label::after {
                    position: absolute;
                    right: 0;
                    top: 0;
                    display: block;
                    width: 3em;
                    height: 3em;
                    /*line-height: 3;*/
                    text-align: center;
                    -webkit-transition: all .35s;
                    -o-transition: all .35s;
                    transition: all .35s;
                    }

                    .accordion input[type=checkbox] + label::after {
                    content: "+";
                    }

                    .accordion input[type=checkbox]:checked + label::after {
                    transform: rotate(315deg);
                    }

                    /*Accordion customisations*/
                    .accordion .tab {
                    /*color: inherit;*/
                    }

                    .accordion .tab-content p {
                    padding-left: 1em;
                    }

                    .accordion input:checked ~ .tab-content {
                    max-height: initial;
                    }

                    .accordion label::after {
                    width: 1.5em;
                    height: 1.5em;
                    line-height: 1.5;
                    font-size: 2em;
                    }

                    .accordion label {
                    /*line-height: 3; 4;*/
                    padding: 1em 2em 1em 1em;
                    }

                    .accordion.red label {
                    background-color: rgba(237, 20, 61, 0.58); /*crimson;*/
                    }

                    .accordion.red .tab-content {
                    background-color: rgba(237, 20, 61, 0.48); /*#EC2F55;*/
                    }

                    .accordion.tech label {
                    border-left: 1px solid gainsboro;
                    border-top: 1px solid gainsboro;
                    border-right: 1px solid gainsboro;
                    background-color: inherit;
                    color: dimgrey;
                    }

                    .accordion.tech .tab-content {
                    border-left: 1px solid gainsboro;
                    border-bottom: 1px solid gainsboro;
                    border-right: 1px solid gainsboro;
                    background-color: rgb(246, 251, 243);
                    }

                    .accordion.tech .tab {
                    color: inherit;
                    }

                    .accordion.error label {
                    border-left: 1px solid #c1ccb2;
                    border-top: 1px solid #c1ccb2;
                    border-right: 1px solid #c1ccb2;
                    background-color: inherit;
                    color: crimson;
                    }

                    .accordion.error .tab-content {
                    border-left: 1px solid #c1ccb2;
                    border-bottom: 1px solid #c1ccb2;
                    border-right: 1px solid #c1ccb2;
                    background-color: rgb(246, 251, 243);
                    }

                    .accordion.error .tab {
                    color: inherit;
                    }

                    .accordion.error .fail-message {
                    font-weight: normal;
                    }


                    /*#084978;*/
                    .arrow {
                    margin: 1em;
                    position: relative;
                    width: 0;
                    height: 0;
                    border-top: 1em solid transparent;
                    border-right: 1em solid cadetblue;
                    -webkit-transform: rotate(320deg); /*220deg*/
                    -moz-transform: rotate(320deg);
                    -ms-transform: rotate(320deg);
                    -o-transform: rotate(320deg);
                    transform: rotate(320deg);
                    }
                    .arrow:after {
                    content: "";
                    position: absolute;
                    border: 0 solid transparent;
                    border-top: 0.5em solid cadetblue;
                    border-radius: 0 2.5em 0 0;
                    top: -2.5em;
                    left: -0.2em;
                    width: 1.5em;
                    height: 1.5em;
                    -webkit-transform: rotate(225deg); /*45deg*/
                    -moz-transform: rotate(225deg);
                    -ms-transform: rotate(225deg);
                    -o-transform: rotate(225deg);
                    transform: rotate(225deg);
                    }

                    /*Customisations to show arrow in first child*/
                    .arrow {
                    float:left;
                    margin-left:1.8em;
                    margin-top:0.6em;
                    }
                    /*First nested child customisations. Note: first-of-type does not work on class so instead adjust all children and then correct all but the first one.*/
                    .nested-child {
                    margin-top: 1.4em;
                    }
                    .nested-child ~ .nested-child {
                    margin-top: initial;
                    }


                </style>

            </head>
            <body>


                <svg style="position: absolute; width: 0; height: 0; overflow: hidden" version="1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px" viewBox="0 0 512 512" xml:space="preserve">
        <defs>
            <symbol id="icon-lemon-2" viewBox="0 0 512 512">
                <path style="fill:#F4DE3B;" d="M452.541,261.129c0-100.923-63.981-185.539-150.123-208.285c0.254-1.975,0.399-3.985,0.399-6.029
	                C302.816,20.961,281.856,0,256.001,0s-46.816,20.961-46.816,46.817c0,2.044,0.145,4.054,0.399,6.029
	                C123.441,75.592,59.459,160.206,59.459,261.129c0,100.844,63.881,185.405,149.921,208.231c2.116,23.896,22.175,42.635,46.621,42.635
	                s44.505-18.739,46.621-42.635C388.66,446.534,452.541,361.973,452.541,261.129z" />
                <path style="fill:#D6BD27;" d="M256.004,512c-24.446,0-44.503-18.748-46.621-42.643C123.341,446.528,59.46,361.974,59.46,261.136
	                c0-100.928,63.983-185.546,150.129-208.286c-0.257-1.976-0.411-3.991-0.411-6.031c0-25.858,20.968-46.813,46.826-46.813
	                c-8.957,0-16.22,20.956-16.22,46.813c0,2.04,0.051,4.055,0.141,6.031c-29.823,22.739-51.985,107.357-51.985,208.286
	                c0,100.839,22.123,185.392,51.921,208.222C240.593,493.252,247.535,512,256.004,512z" />
            </symbol>
        </defs>
    </svg>

                <svg style="position: absolute; width: 0; height: 0; overflow: hidden" version="1.1" viewBox="0 0 612 792" xml:space="preserve" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink">
        <style type="text/css">
            .st0 {
                clip-path: url(#SVGID_2_);
                fill: none;
                stroke: crimson;
                stroke-width: 45;
            }

            .st1 {
                fill: crimson; /*#E44061;*/
            }
        </style>
        <symbol id="icon-error-x" viewBox="0 0 612 792">
        <g>
        <g>
        <defs>
            <rect height="512" id="SVGID_1_" width="512" x="50" y="140" />
        </defs>
        <clipPath id="SVGID_2_">
        <use style="overflow:visible;" xlink:href="#SVGID_1_" />
        </clipPath>
        <path class="st0" d="M306,629.5c128.8,0,233.5-104.7,233.5-233.5S434.8,162.5,306,162.5S72.5,267.2,72.5,396    S177.2,629.5,306,629.5L306,629.5z" />
        </g>
        <polygon class="st1" points="348.7,396 448,296.7 405.3,254 306,353.3 206.7,254 164,296.7 263.3,396 164,495.3 206.7,538    306,438.7 405.3,538 448,495.3 348.7,396  " />
        </g>
        </symbol>
    </svg>


                <div class="report-assembly">

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
                            <ul class="menu">
                                <xsl:for-each select="Assembly/Definition">

                                  <xsl:sort select="Name"/>

                                    <li class="menu-item">
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
                        </xsl:if>

                    </div>
                    <!--report-header-->

                    <div class="report-body">

                        <xsl:for-each select="Assembly/Definition">

                          <xsl:sort select="Name"/>

                            <a name="{generate-id(Name)}"></a>
                            <section class="section-scenario">
                                <xsl:if test="descendant::Failure">
                                    <xsl:attribute name="class">section-scenario status-failure</xsl:attribute>
                                    <svg class="icon icon-lemon-2 jello animated">
                                        <use xlink:href="#icon-lemon-2"></use>
                                    </svg>
                                    <!--::THERE IS A FAILURE::-->
                                </xsl:if>

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

            </body>
        </html>
    </xsl:template>


</xsl:stylesheet>

