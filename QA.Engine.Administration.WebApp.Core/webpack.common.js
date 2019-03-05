const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const ForkTsCheckerWebpackPlugin = require('fork-ts-checker-webpack-plugin');

module.exports = {
    entry: {
        polyfill: '@babel/polyfill',
        main: './ClientApp/index.tsx'
    },
    output: {
        filename: '[name].bundle.js',
        path: path.join(__dirname, 'wwwroot/dist'),
        publicPath: '/'
    },
    resolve: {
        extensions: ['.ts', '.tsx', '.js', '.json'],
        modules: ['node_modules'],
        alias: {
            components: path.resolve(__dirname, 'ClientApp/components/'),
            constants: path.resolve(__dirname, 'ClientApp/constants/'),
            enums: path.resolve(__dirname, 'ClientApp/enums/'),
            services: path.resolve(__dirname, 'ClientApp/services/'),
            stores: path.resolve(__dirname, 'ClientApp/stores/'),
            assets: path.resolve(__dirname, 'ClientApp/assets/'),
            qp: path.resolve(__dirname, 'ClientApp/qp/'),
        }
    },
    module: {
        rules: [
            {
                test: /\.(jpg|jpeg|png|gif|svg)?$/,
                exclude: /fonts/,
                use: {
                    loader: 'url-loader',
                    options: {
                        name: 'img/[name].[ext]',
                        limit: 10000
                    }
                }
            },
            {
                test: /\.(woff|woff2|eot|ttf|otf|svg)?$/,
                exclude: /img/,
                use: {
                    loader: 'url-loader',
                    options: {
                        name: 'fonts/[name].[ext]',
                        limit: 10000
                    }
                }
            },
        ]
    },
    plugins: [
        new ForkTsCheckerWebpackPlugin(),
        new HtmlWebpackPlugin({
            title: 'Manage Site',
            template: 'ClientApp/assets/index.html'
        }),
        CopyWebpackPlugin([{
            from: './ClientApp/assets/pmrpc.js',
            to: './scripts'
        }])
    ],
};
