(function (global) {
    System.config({
        paths: {
            // paths serve as alias
            'npm:': '../node_modules/'
        },
        // map tells the System loader where to look for things
        map: {
            // our app is within the app folder
            'app': 'app',

            // angular bundles
            '@angular/animations': 'npm:@angular/animations/bundles/animations.umd.js',
            '@angular/animations/browser': 'npm:@angular/animations/bundles/animations-browser.umd.js',
            '@angular/core': 'npm:@angular/core/bundles/core.umd.js',
            '@angular/common': 'npm:@angular/common/bundles/common.umd.js',
            '@angular/compiler': 'npm:@angular/compiler/bundles/compiler.umd.js',
            '@angular/platform-browser': 'npm:@angular/platform-browser/bundles/platform-browser.umd.js',
            '@angular/platform-browser/animations': 'npm:@angular/platform-browser/bundles/platform-browser-animations.umd.js',
            '@angular/platform-browser-dynamic': 'npm:@angular/platform-browser-dynamic/bundles/platform-browser-dynamic.umd.js',
            '@angular/http': 'npm:@angular/http/bundles/http.umd.js',
            '@angular/router': 'npm:@angular/router/bundles/router.umd.js',
            '@angular/router/upgrade': 'npm:@angular/router/bundles/router-upgrade.umd.js',
            '@angular/forms': 'npm:@angular/forms/bundles/forms.umd.js',
            '@angular/upgrade': 'npm:@angular/upgrade/bundles/upgrade.umd.js',
            '@angular/upgrade/static': 'npm:@angular/upgrade/bundles/upgrade-static.umd.js',

            // other libraries
            'rxjs': 'npm:rxjs',
            'angular-in-memory-web-api': 'npm:angular-in-memory-web-api/bundles/in-memory-web-api.umd.js',
            'angular2-datatable': 'npm:angular2-datatable',
            'lodash': 'npm:lodash/lodash.js',
            
            'angular2-busy': 'npm:angular2-busy',
            'xont-ventura-message-prompt': 'npm:xont-ventura-message-prompt/xont-ventura-message-prompt.umd.js',
            'xont-ventura-single-prompt': 'npm:xont-ventura-single-prompt/xont-ventura-single-prompt.umd.js',
            'xont-ventura-multiple-prompt': 'npm:xont-ventura-multiple-prompt/xont-ventura-multiple-prompt.umd.js',
            'xont-ventura-datepicker': 'npm:xont-ventura-datepicker/xont-ventura-datepicker.umd.js',
            'xont-ventura-validators': 'npm:xont-ventura-validators/xont-ventura-validators.umd.js',
            'xont-ventura-gridexport': 'npm:xont-ventura-gridexport/xont-ventura-gridexport.umd.js',
            'xont-ventura-classification-selector': 'npm:xont-ventura-classification-selector/xont-ventura-classification-selector.umd.js',
            'xont-ventura-services': 'npm:xont-ventura-services/xont-ventura-services.umd.js',
            'xont-ventura-collapsible': 'npm:xont-ventura-collapsible/xont-ventura-collapsible.umd.js',
            'xont-ventura-gridloader': 'npm:xont-ventura-gridloader/xont-ventura-gridloader.umd.js',
            'xont-ventura-list-prompt': 'npm:xont-ventura-list-prompt/xont-ventura-list-prompt.umd.js',
            'xont-ventura-date-controller': 'npm:xont-ventura-date-controller/xont-ventura-date-controller.umd.js'

        },
        
        packages: {
                        app: {
                            main: 'main.js'
                        },
                        'angular2-datatable': {
                            main: 'index.js',
                            defaultExtension: 'js'
                        },

                        'angular2-busy': {
                            main: './index.js',
                            defaultExtension: 'js'
                        },
                        
                        rxjs: {
                            defaultExtension: 'js'
                        }
                    }

    });
})(this);
